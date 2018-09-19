#include "Target.h"


Target::Target(Rect r,Point p,int t) {
	centre = p;
	bbox = r;
	life =2;
	tolerance = t;
	id = -1;
}

void Target::addLife() {
	life++;
}
void Target::die() {
	life--;
}
// if the new centre is nearby to the the previously
// recorded ones, consider it the same
bool Target::isSame(Target t) {
	return (abs(t.centre.x - centre.x) < tolerance) && (abs(t.centre.y - centre.y) < tolerance);
}

int Target::getLife() {
	return life;
}

Point Target::getCentre() {
	return centre;
}

void Target::setId(int i) {
	id = i;
}

int Target::getId() {
	return id;
}
