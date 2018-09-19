#include "opencv2/videoio/videoio.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include "opencv2/features2d.hpp"

#include <iostream>

#include "osc/OscOutboundPacketStream.h"
#include "ip/UdpSocket.h"

#include "Targets.h"

using namespace cv;
using namespace std;

#define ADDRESS "127.0.0.1"
#define PORT 7000
#define OUTPUT_BUFFER_SIZE 1024
UdpTransmitSocket transmitSocket(IpEndpointName(ADDRESS, PORT));


static void showFrames(VideoCapture capture);
static void blobDetect(Mat& image);
static void sendOSC(vector<Rect> boundRect, int rows, int cols);
Targets targets;

// clip the depth map to certain range to remove background
uint16_t minDistance = 10;
uint16_t maxDistance = 3000;//2000; //measured in mm
int minArea = 0;
int targetTolerance = 75;


/*
* To work with Kinect or XtionPRO the user must install OpenNI library and PrimeSensorModule for OpenNI and
* configure OpenCV with WITH_OPENNI flag is ON (using CMake).
*/
int main(int argc, char* argv[])
{
	int imageMode = 0; // image mode: resolution and fps, supported three values:  0 - CAP_OPENNI_VGA_30HZ, 
	                   // 1 - CAP_OPENNI_SXGA_15HZ,"  2 - CAP_OPENNI_SXGA_30HZ 
	string filename; // Filename of .oni video file. The data will grabbed from it
	bool isVideoReading = 0;

	cout << "Device opening ..." << endl;
	VideoCapture capture;
	if (isVideoReading)
		capture.open(filename);
	else
	{
		capture.open(CAP_OPENNI2);
		if (!capture.isOpened())
			capture.open(CAP_OPENNI);
	}

	cout << "done." << endl;

	if (!capture.isOpened())
	{
		cout << "Can not open a capture object." << endl;
		return -1;
	}

	if (!isVideoReading && imageMode >= 0)
	{
		bool modeRes = false;
		modeRes = capture.set(CAP_OPENNI_IMAGE_GENERATOR_OUTPUT_MODE, CAP_OPENNI_VGA_30HZ);
		if (!modeRes)
			cout << "\nThis image mode is not supported by the device, the default value (CV_CAP_OPENNI_SXGA_15HZ) will be used.\n" << endl;
	}

	// turn on depth img
	capture.set(CAP_OPENNI_DEPTH_GENERATOR_PRESENT, true);
	// turn on colour img
	capture.set(CAP_OPENNI_IMAGE_GENERATOR_PRESENT, true);

	// Print some avalible device settings.
	if (capture.get(CAP_OPENNI_DEPTH_GENERATOR_PRESENT))
	{
		cout << "\nDepth generator output mode:" << endl <<
			"FRAME_WIDTH      " << capture.get(CAP_PROP_FRAME_WIDTH) << endl <<
			"FRAME_HEIGHT     " << capture.get(CAP_PROP_FRAME_HEIGHT) << endl <<
			"FRAME_MAX_DEPTH  " << capture.get(CAP_PROP_OPENNI_FRAME_MAX_DEPTH) << " mm" << endl <<
			"FPS              " << capture.get(CAP_PROP_FPS) << endl <<
			"REGISTRATION     " << capture.get(CAP_PROP_OPENNI_REGISTRATION) << endl;
	}
	else
	{
		cout << "\nDevice doesn't contain depth generator or it is not selected." << endl;
	}


	//cout << "argc " << argc << "\n";
	if (argc == 5) {
		minDistance = strtol(argv[1], NULL, 0);
		maxDistance = strtol(argv[2], NULL, 0);
		minArea = strtol(argv[3], NULL, 0);
		targetTolerance = strtol(argv[4], NULL, 0);
	}
	cout << "mininum distance: " << minDistance << "\n";;
	cout << "maximum distance: " << maxDistance << "\n";
	cout << "min area: " << minArea << "\n";
	cout << "blob tolerance: " << targetTolerance << "\n";

	showFrames(capture);
	return 0;
}

static void sendOSC(int rows, int cols) {
	char buffer[OUTPUT_BUFFER_SIZE];
	osc::OutboundPacketStream p(buffer, OUTPUT_BUFFER_SIZE);

	p << osc::BeginBundleImmediate
		<< osc::BeginMessage("/numPoints")
		<< (int)targets.getNumTargets()
		<< osc::EndMessage
		<< osc::BeginMessage("/points");


	// origin needs to be changed since opencv origin is top left,
	// unity is bottom left
	for (int i = 0; i < targets.getNumTargets(); i++) {
		float centreX = cols - (targets.getTarget(i).getCentre().x) ; //col - to flip for projection mode 8
		float centreY = rows - (targets.getTarget(i).getCentre().y);
		//if(i == 0) cout << centreX << "...." << centreY << "\n";
		p << (float)centreX;
		p << (float)centreY;
	}
	p << osc::EndMessage << osc::EndBundle;
	transmitSocket.Send(p.Data(), p.Size());
}

static void blobDetect(Mat& image) {

	float scaleFactor = 0.5f; //0.1f; //for colormap

	
	for (int y = 0; y < image.rows; y++)
	{
		for (int x = 0; x < image.cols; x++)
		{
			uint16_t  p = image.at<uint16_t>(y, x);
			if (p < maxDistance && p > minDistance) {
				image.at<uint16_t>(y, x) = p ;
			}
			else {
				image.at<uint16_t>(y, x) = 0;
			}

		}
	}

	image.convertTo(image, CV_8UC1, scaleFactor);
	//convert to colour to help distinguish overlapping objects (of different depth)
	//applyColorMap(image, image, COLORMAP_JET);


	// rotate img
	//rotate(image, image, ROTATE_90_COUNTERCLOCKWISE);

	//down scale image to increase performance
	resize(image, image, Size(image.cols/2,image.rows/2));
	imshow("Raw Image", image);

	// Apply dilation to reduce noise
	int dilation_size = 3;
	Mat element = getStructuringElement(MORPH_ELLIPSE,
		Size(2 * dilation_size + 1, 2 * dilation_size + 1),
		Point(dilation_size, dilation_size));
	dilate(image, image, element);

	//blur
	medianBlur(image,image, 31);
	imshow("Smoothed Image", image);

	// Find Blobs by finding contours and calculate bounding boxes
	Mat canny_output;
	vector<vector<Point> > contours;
	vector<Vec4i> hierarchy;
	int thresh = 100;

	// Detect edges using canny & find contours
	Canny(image, canny_output, thresh, thresh * 2, 3);
	//imshow("canny output", canny_output);
	findContours(canny_output, contours, hierarchy, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_TC89_KCOS, Point(0, 0));

	// Draw bounding boxes
	vector<vector<Point> > contours_poly(contours.size());
	Mat drawing = Mat::zeros(canny_output.size(), CV_8UC3);

	for (int i = 0; i< contours.size(); i++)
	{
		approxPolyDP(Mat(contours[i]), contours_poly[i], 3, true);
		Rect boundRect = boundingRect(Mat(contours_poly[i]));
		double area = boundRect.height * boundRect.width;

		//remove small boxes
		if (area > minArea) { //1000 original

			float centreX = (boundRect.x + boundRect.width / 2) ;
			float centreY = (boundRect.y + boundRect.height / 2) ;
			Point centre = Point(centreX,centreY);

			Target newTarget = Target(boundRect, centre,targetTolerance);
			int id = targets.addTarget(newTarget);
			
			if (id > targets.getNumTargets() || id < 0) id = -1;
			putText(drawing, std::to_string(id), centre, FONT_HERSHEY_PLAIN, 2, Scalar(255, 255, 255, 255), 2);

			Scalar color = Scalar(255, 255, 255);
			drawContours(drawing, contours_poly, i, color, 1, 8, vector<Vec4i>(), 0, Point());
			rectangle(drawing, boundRect.tl(), boundRect.br(), color, 2, 8, 0);
			//circle(drawing, center[i], (int)radius[i], color, 2, 8, 0);
		}
	}

	/// Show in a window
	namedWindow("Contours", CV_WINDOW_AUTOSIZE);
	sendOSC(drawing.rows, drawing.cols);
	imshow("Contours", drawing);

}

static void showFrames(VideoCapture capture) {
	for (;;)
	{
		Mat depthMap;
		Mat colourImage;

		if (!capture.grab())
		{
			cout << "Can not grab images." << endl;
			return;
		}
		else
		{
			if (capture.retrieve(depthMap, CAP_OPENNI_DEPTH_MAP))
			{
				//const float scaleFactor = 0.05f;
				//Mat show; 
				//depthMap.convertTo(show, CV_8UC1, scaleFactor);
				//imshow("depth map", show);
				blobDetect(depthMap);
			}

			if (capture.retrieve(colourImage, CAP_OPENNI_BGR_IMAGE)) {
				// rotate img
				//rotate(colourImage, colourImage, ROTATE_90_COUNTERCLOCKWISE);
				imshow("rgb image", colourImage);
			}
				

			targets.ageTargets();
		}

		if (waitKey(30) >= 0)
			break;
	}
}