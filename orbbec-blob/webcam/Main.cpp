#include "opencv2/videoio/videoio.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include "opencv2/features2d.hpp"

#include <iostream>

#include "osc/OscOutboundPacketStream.h"
#include "ip/UdpSocket.h"

using namespace cv;
using namespace std;

#define ADDRESS "127.0.0.1"
#define PORT 7000
#define OUTPUT_BUFFER_SIZE 1024
UdpTransmitSocket transmitSocket(IpEndpointName(ADDRESS, PORT));


static void showFrames(VideoCapture capture);
static void blobDetect(Mat& image);
static void sendOSC(vector<Rect> boundRect, int rows, int cols);


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

	// turn on depth
	capture.set(CAP_OPENNI_DEPTH_GENERATOR_PRESENT, true);

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

	showFrames(capture);
	return 0;
}

static void sendOSC(vector<Rect> boundRect, int rows, int cols) {
	char buffer[OUTPUT_BUFFER_SIZE];
	osc::OutboundPacketStream p(buffer, OUTPUT_BUFFER_SIZE);

	p << osc::BeginBundleImmediate
		<< osc::BeginMessage("/numPoints")
		<< (int)boundRect.size()
		<< osc::EndMessage
		<< osc::BeginMessage("/points");

	for (int i = 0; i < boundRect.size(); i++) {
		float centreX = (boundRect[i].x + boundRect[i].width / 2) / (float)cols;
		float centreY = (boundRect[i].y + boundRect[i].height / 2) / (float)rows;
		p << (float)centreX;
		p << (float)centreY;
	}
	p << osc::EndMessage << osc::EndBundle;
	transmitSocket.Send(p.Data(), p.Size());
}

static void blobDetect(Mat& image) {

	float scaleFactor = 0.9f;

	// clip the depth map to certain range to remove background
	uint16_t minDistance = 10;
	uint16_t maxDistance = 1000; //measured in mm
	
	for (int y = 0; y < image.rows; y++)
	{
		for (int x = 0; x < image.cols; x++)
		{
			uint16_t  p = image.at<uint16_t>(y, x);
			if (p > maxDistance && p > minDistance) {
				image.at<uint16_t>(y, x) = 0;
			}

		}
	}

	image.convertTo(image, CV_8UC1, scaleFactor);

	//down scale image to increase performance
	resize(image, image, Size(image.cols/2,image.rows/2));

	// Apply dilation to reduce noise
	int dilation_size = 5;
	Mat element = getStructuringElement(MORPH_ELLIPSE,
		Size(2 * dilation_size + 1, 2 * dilation_size + 1),
		Point(dilation_size, dilation_size));
	dilate(image, image, element);

	// Apply Blur to reduce noise further
	GaussianBlur(image, image, Size(11, 11), 0, 0);

	// Find Blobs by finding contours and calculate bounding boxes
	Mat threshold_output;
	vector<vector<Point> > contours;
	vector<Vec4i> hierarchy;
	int thresh = 100;

	// Detect edges using Threshold
	threshold(image, threshold_output, thresh, 255, THRESH_BINARY);
	// Find contours
	findContours(threshold_output, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

	// Approximate contours to polygons + get bounding rects and circles
	vector<vector<Point> > contours_poly(contours.size());
	vector<Rect> boundRect(contours.size());
	vector<Point2f>center(contours.size());
	vector<float>radius(contours.size());

	for (int i = 0; i < contours.size(); i++)
	{
		approxPolyDP(Mat(contours[i]), contours_poly[i], 3, true);
		boundRect[i] = boundingRect(Mat(contours_poly[i]));
		//minEnclosingCircle((Mat)contours_poly[i], center[i], radius[i]);
	}

	/// Draw polygonal contour + bonding rects + circles
	Mat drawing = Mat::zeros(threshold_output.size(), CV_8UC3);
	for (int i = 0; i< contours.size(); i++)
	{
		Point textOrg = (boundRect[i].tl(), boundRect[i].br());
		putText(drawing, std::to_string(i), textOrg, FONT_HERSHEY_PLAIN, 2, Scalar(255, 255, 255, 255), 2);

		Scalar color = Scalar(255, 255, 255);
		drawContours(drawing, contours_poly, i, color, 1, 8, vector<Vec4i>(), 0, Point());
		rectangle(drawing, boundRect[i].tl(), boundRect[i].br(), color, 2, 8, 0);
		//circle(drawing, center[i], (int)radius[i], color, 2, 8, 0);
	}

	/// Show in a window
	namedWindow("Contours", CV_WINDOW_AUTOSIZE);
	sendOSC(boundRect, drawing.rows, drawing.cols);
	imshow("Contours", drawing);

}

static void showFrames(VideoCapture capture) {
	for (;;)
	{
		Mat depthMap;

		if (!capture.grab())
		{
			cout << "Can not grab images." << endl;
			return;
		}
		else
		{
			if (capture.retrieve(depthMap, CAP_OPENNI_DEPTH_MAP))
			{
				const float scaleFactor = 0.05f;
				Mat show; 
				depthMap.convertTo(show, CV_8UC1, scaleFactor);
				//imshow("depth map", show);
				blobDetect(depthMap);
			}

		}

		if (waitKey(30) >= 0)
			break;
	}
}