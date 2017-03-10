/*#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/nonfree/nonfree.hpp"
#include "opencv/highgui.h"
using namespace cv;
int main(int argc, char** argv)
{
	Mat img = imread("../test.jpg");
	SIFT sift;  //实例化 SIFT 类
	vector<KeyPoint> key_points;	//特征点
									// descriptors 为描述符，mascara 为掩码矩阵
	Mat descriptors, mascara;
	Mat output_img;					//输出图像矩阵
	sift(img, mascara, key_points, descriptors);	//执行 SIFT 运算
	//在输出图像中绘制特征点
	drawKeypoints(img,		//输入图像
		key_points,			//特征点矢量
		output_img,			//输出图像
		Scalar::all(-1),	//绘制特征点的颜色，为随机
							//以特征点为中心画圆，圆的半径表示特征点的大小，直线表示特征点的方向
		DrawMatchesFlags::DRAW_RICH_KEYPOINTS);
	namedWindow("SIFT");
	imshow("SIFT", output_img);
	waitKey(0);
	return 0;
}*/

#include "opencv2/core/core.hpp"
#include "opencv/highgui.h"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/nonfree/nonfree.hpp"
#include "opencv2/legacy/legacy.hpp"
using namespace cv;
using namespace std;
int main(int argc, char** argv)
{
	//待匹配的两幅图像，其中 img1 包括 img2，也就是要从 img1 中识别出 img2
	Mat img1 = imread("../box_in_scene.png");
	Mat img2 = imread("../box.png");
	SIFT sift1, sift2;
	vector<KeyPoint> key_points1, key_points2;
	Mat descriptors1, descriptors2, mascara;
	sift1(img1, mascara, key_points1, descriptors1);
	sift2(img2, mascara, key_points2, descriptors2);

	//实例化暴力匹配器——BruteForceMatcher
	BruteForceMatcher<L2<float>> matcher;
	//定义匹配器算子
	vector<DMatch>matches;
	//实现描述符之间的匹配，得到算子 matches
	matcher.match(descriptors1, descriptors2, matches);
	//提取出前 30 个最佳匹配结果
	std::nth_element(matches.begin(),	//匹配器算子的初始位置
		matches.begin() + 29,			// 排序的数量
		matches.end());					// 结束位置
	//剔除掉其余的匹配结果
	matches.erase(matches.begin() + 30, matches.end());
	namedWindow("SIFT_matches");
	Mat img_matches;
	//在输出图像中绘制匹配结果
	drawMatches(img1, key_points1,		//第一幅图像和它的特征点
		img2, key_points2,				//第二幅图像和它的特征点
		matches,						//匹配器算子
		img_matches,					//匹配输出图像
		Scalar(255, 255, 255));			//用白色直线连接两幅图像中的特征点
	imshow("SIFT_matches", img_matches);
	waitKey(0);
	return 0;
}


