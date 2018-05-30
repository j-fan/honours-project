#include "Target.h"

class Targets {
private:
	std::vector<Target> targetList;
public:
	int addTarget(Target);
	void ageTargets();
	int getNumTargets();
	Target getTarget(int i);
};
#pragma once
