//---------------------------------【头文件、命名空间包含部分】----------------------------
//		描述：包含程序所使用的头文件和命名空间
//------------------------------------------------------------------------------------------------
#include <opencv2/opencv.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/nonfree/features2d.hpp>
#include <opencv2/features2d/features2d.hpp>
#include <windows.h> 
#include <iostream> 
using namespace cv;
using namespace std;



//-----------------------------------【ShowHelpText( )函数】----------------------------------
//          描述：输出一些帮助信息
//----------------------------------------------------------------------------------------------
static void ShowHelpText()
{
	//输出欢迎信息和OpenCV版本
	printf("\n\n\t\t\t非常感谢购买《OpenCV3编程入门》一书！\n");
	printf("\n\n\t\t\t此为本书OpenCV2版的第72个配套示例程序\n");
	printf("\n\n\t\t\t   当前使用的OpenCV版本为：" CV_VERSION);
	printf("\n\n  ----------------------------------------------------------------------------\n");
	//输出一些帮助信息
	printf("\n\t欢迎来到【ORB算法描述提取，配合FLANN-LSH进行匹配】示例程序\n");
	printf("\n\n\t按键操作说明: \n\n"
		"\t\t键盘按键【ESC】- 退出程序\n");
}


//--------------------------------------【main( )函数】-----------------------------------------
//          描述：控制台应用程序的入口函数，我们的程序从这里开始执行
//-----------------------------------------------------------------------------------------------
int main()
{
	//【0】改变console字体颜色
	system("color 2F");

	//【0】显示帮助文字
	//ShowHelpText();

	//【0】载入源图，显示并转化为灰度图
	Mat srcImage = imread("../d.bmp");
	//imshow("原始图", srcImage);
	Mat grayImage;
	cvtColor(srcImage, grayImage, CV_BGR2GRAY);

	//------------------检测SIFT特征点并在图像中提取物体的描述符----------------------

	//【1】参数定义
	OrbFeatureDetector featureDetector(500, 1.9f, 8, 32, 0, 2, ORB::FAST_SCORE, 31);
	vector<KeyPoint> keyPoints;
	Mat descriptors;

	double start = GetTickCount();
	//【2】调用detect函数检测出特征关键点，保存在vector容器中
	featureDetector.detect(grayImage, keyPoints);
	//【3】计算描述符（特征向量）
	OrbDescriptorExtractor featureExtractor;
	featureExtractor.compute(grayImage, keyPoints, descriptors);

	double end = GetTickCount();
	cout << end - start << endl;
	Mat output_img;			//输出图像矩阵
	drawKeypoints(srcImage,		//输入图像
		keyPoints,			//特征点矢量
		output_img,			//输出图像
		Scalar::all(-1),	//绘制特征点的颜色，为随机
		//以特征点为中心画圆，圆的半径表示特征点的大小，直线表示特征点的方向
		DrawMatchesFlags::DEFAULT);
	namedWindow("ORB");
	imshow("ORB", output_img);
	cout << descriptors.rows << endl;
	imwrite("d_orb.bmp", output_img);
	waitKey(0);
	return 0;
}

/*#include <iostream>    
#include "opencv2/core/core.hpp"    
#include "opencv2/features2d/features2d.hpp"    
#include "opencv2/highgui/highgui.hpp"   
#include<opencv2/legacy/legacy.hpp>  
#include <vector>    
#include<windows.h>  
using namespace cv;
using namespace std;
int main()
{
	Mat img = imread("../flower.jpg");
	ORB orb(500);
	vector<KeyPoint> keyPoints;
	Mat descriptors;
	double start = GetTickCount();
	orb(img, Mat(), keyPoints, descriptors);
	double end = GetTickCount();
	cout << end - start << endl;
	Mat output_img;			//输出图像矩阵
	drawKeypoints(img,		//输入图像
		keyPoints,			//特征点矢量
		output_img,			//输出图像
		Scalar::all(-1),	//绘制特征点的颜色，为随机
		//以特征点为中心画圆，圆的半径表示特征点的大小，直线表示特征点的方向
		DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS);
	namedWindow("ORB");
	imshow("ORB", output_img);
	cout << descriptors.rows << endl;
	imwrite("flower_orb.jpg", output_img);
	waitKey(0);
	return 0;
}

/*#include <iostream>    
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
	Mat img_1 = imread("../IMG_1.jpg");
	Mat img_2 = imread("../IMG_2.jpg");
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
}*/