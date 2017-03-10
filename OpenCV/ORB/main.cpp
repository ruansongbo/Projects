#include <iostream>    
#include "opencv2/core/core.hpp"    
#include "opencv2/features2d/features2d.hpp"    
#include "opencv2/highgui/highgui.hpp"   
#include<opencv2/legacy/legacy.hpp>  
#include <iostream>    
#include <vector>    
#include<windows.h>  
using namespace cv;
using namespace std;
int main()
{
	Mat img_1 = imread("../box_in_scene.png");
	Mat img_2 = imread("../box.png");
	if (!img_1.data || !img_2.data)
	{
		cout << "error reading images " << endl;
		return -1;
	}

	ORB orb;
	vector<KeyPoint> keyPoints_1, keyPoints_2;
	Mat descriptors_1, descriptors_2;
	double start = GetTickCount();
	orb(img_1, Mat(), keyPoints_1, descriptors_1);
	orb(img_2, Mat(), keyPoints_2, descriptors_2);

	BruteForceMatcher<HammingLUT> matcher;
	vector<DMatch> matches;
	matcher.match(descriptors_1, descriptors_2, matches);

	double max_dist = 0; double min_dist = 100;
	//-- Quick calculation of max and min distances between keypoints    
	for (int i = 0; i < descriptors_1.rows; i++)
	{
		double dist = matches[i].distance;
		if (dist < min_dist) min_dist = dist;
		if (dist > max_dist) max_dist = dist;
	}
	//printf("-- Max dist : %f \n", max_dist );    
	//printf("-- Min dist : %f \n", min_dist );    
	//-- Draw only "good" matches (i.e. whose distance is less than 0.6*max_dist )    
	//-- PS.- radiusMatch can also be used here.    
	std::vector< DMatch > good_matches;
	for (int i = 0; i < descriptors_1.rows; i++)
	{
		if (matches[i].distance < 0.6*max_dist)
		{
			good_matches.push_back(matches[i]);
		}
	}
	double end = GetTickCount();
	cout << end - start << endl;
	Mat img_matches;
	drawMatches(img_1, keyPoints_1, img_2, keyPoints_2,
		good_matches, img_matches, Scalar::all(-1), Scalar::all(-1),
		vector<char>(), DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS);
	imshow("Match", img_matches);
	cvWaitKey();
	return 0;
}