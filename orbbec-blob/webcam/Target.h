#include "opencv2/features2d.hpp"
#include <iostream>
using namespace cv;

class Target {
private:
	Rect bbox;
	Point centre;
	int life;
	int id;
	int tolerance = 75;
public:
	Target(Rect r, Point p, int t);
	void addLife();
	void die();
	// if the new centre is nearby to the the previously
	// recorded ones, consider it the same
	bool isSame(Target t);
	int getLife();
	Point getCentre();
	void setId(int i);
	int getId();
}; 
#pragma once
