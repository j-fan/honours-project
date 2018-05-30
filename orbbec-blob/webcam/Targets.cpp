#include "Targets.h"


int Targets::addTarget(Target t) {
	int toReplace = -1;
	for (int i = 0; i < targetList.size(); i++) {
		if (targetList[i].isSame(t)) {
			toReplace = i;
			break;
		}
	}
	if (toReplace >= 0) {
		t.setId(toReplace);
		targetList[toReplace] = t;
	}
	else {
		t.setId(targetList.size());
		targetList.push_back(t);
	}

	return toReplace >= 0 ? toReplace : targetList.size();
}
void Targets::ageTargets() {

	for (int i = targetList.size() - 1; i >= 0; i--) {
		targetList.at(i).die();
		if (targetList.at(i).getLife() <= 0) {
			targetList.erase(targetList.begin() + i);
		}
	}
}

int Targets::getNumTargets() {
	return targetList.size();
}

Target Targets::getTarget(int i) {
	return targetList[i];
}