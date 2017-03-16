#include "opencv2/core/core.hpp"
#include "opencv/highgui.h"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/nonfree/nonfree.hpp"
#include <windows.h>  
#include <iostream>  
using namespace cv;
using namespace std;
int main(int argc, char** argv)
{
	Mat img = imread("../flower.jpg");
	SURF surf(1000.);			//设置行列式阈值为 3000
	vector<KeyPoint> key_points;
	Mat descriptors, mascara;
	Mat output_img;
	double start = GetTickCount();
	surf(img, mascara, key_points, descriptors);
	double end = GetTickCount();
	cout << end - start << endl;
	drawKeypoints(img, key_points, output_img, Scalar::all(-1),
		DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS);
	namedWindow("SURF");
	imshow("SURF", output_img);
	cout << descriptors.rows << endl;
	imwrite("flower_surf.jpg", output_img);
	waitKey(0);
	return 0;
}

/*#include "opencv2/core/core.hpp"
#include "opencv/highgui.h"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/nonfree/nonfree.hpp"
#include "opencv2/legacy/legacy.hpp"
using namespace cv;
using namespace std;
int main(int argc, char** argv)
{
	Mat img1 = imread("../IMG_1.jpg");
	Mat img2 = imread("../IMG_2.jpg");
	SURF surf1(3000.), surf2(3000.);
	vector<KeyPoint> key_points1, key_points2;
	Mat descriptors1, descriptors2, mascara;
	surf1(img1, mascara, key_points1, descriptors1);
	surf2(img2, mascara, key_points2, descriptors2);

	BruteForceMatcher<L2<float>> matcher;
	vector<DMatch>matches;
	matcher.match(descriptors1, descriptors2, matches);
	std::nth_element(matches.begin(), // initial position
		matches.begin() + 49, // position of the sorted element
		matches.end()); // end position
	matches.erase(matches.begin() + 50, matches.end());
	namedWindow("SURF_matches");
	Mat img_matches;
	drawMatches(img1, key_points1,
		img2, key_points2,
		matches,
		img_matches,
		Scalar(255, 255, 255));
	imshow("SURF_matches", img_matches);
	waitKey(0);
	return 0;
}*/