# T_OCR

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tesseract;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace TempOCR
{
    public partial class Form1 : Form
    {
        // 온도에 따른 색상이 두 개이기에 각각 과정을 모두 두 번씩 진행
        // 더 좋은 로직이 있을 것 같으나, 일단 기능 구현.
        // BitOr 연산으로 녹색과 적색 둘 동시 검출 가능하다는 것을 알아냄.
        // 하지만, 녹색과 적색 온도가 겹칠 경우가 생기기에 각 과정을 나눠서 진행하는 것이 맞는 것 같음.
        Bitmap bmp1, bmp2;
        Mat src1, src2;
        // Tesseract data가 적합하지 않아서 LSTM으로 훈련된 Dot 표현 숫자 데이터 사용
        TesseractEngine engine = new TesseractEngine(@"./tessdata", "digits", EngineMode.Default);

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // Button1 클릭 이벤트
            // 컴퓨터 내의 이미지를 pictureBox1에 띄워줌.

            String imgfile = string.Empty;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:/";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                imgfile = dialog.FileName;
            }

            try
            {
                bmp1 = new Bitmap(imgfile);

                pictureBox1.Image = bmp1;
                pictureBox2.Image = bmp1;
            }
            catch { }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            // Button2 클릭 이벤트
            // 온도로 표시되는 특정 색상만을 추출하여 pictureBox1에 띄움.

            // 색상 검출에 쓰이는 Opencv의 Mat 형식을 사용하기 위해 Bitmap 형식을 Mat으로 바꿔줌.
            src1 = BitmapConverter.ToMat(bmp1);
            src2 = BitmapConverter.ToMat(bmp1);
            Mat[] mv1 = new Mat[3];
            Mat[] mv2 = new Mat[3];
            Mat mask1 = new Mat();
            Mat mask2 = new Mat();

            // HSV 사용
            // 색상 추출에 BGR 이미지 형식보다 HSV 이미지 형식이 좋음.
            // BGR 색공간을 HSV로 변환 후 Hue 채널 이미지를 얻기 위해 채널 분리
            // 분리 후 원본 이미지는 다시 BGR 색공간으로 변환
            // Cv2.CvtColor(src, src, ColorConversionCodes.BGR2HSV);
            // mv = Cv2.Split(src);
            // Cv2.CvtColor(src, src, ColorConversionCodes.HSV2BGR);

            // 색상 추출 시 필요한 Hue 값을 기준으로 영역 내 색상을 나타내는 마스크 생성
            // 원본 이미지에 BitwiseAnd 연산을 통해 원하는 색상 추출
            // Cv2.InRange(입력, 최솟값, 최댓값, 출력); Hue 채널을 1채널이기에 1채널 Scalar 사용
            // Cv2.InRange(mv[0], new Scalar(1), new Scalar(1), mask);

            // 특정 색을 추출하기 위해선 HSV보다 RGB로 특정 색을 지정하는 것이 나음.
            // HSV의 경우 특정 색을 가진 이미지 검출에 적합.
            Cv2.CvtColor(src1, src1, ColorConversionCodes.BGR2RGB);
            mv1 = Cv2.Split(src1);
            Cv2.CvtColor(src1, src1, ColorConversionCodes.RGB2BGR);

            // 색상 추출 시 필요한 값을 기준으로 영역 내 색상을 나타내는 마스크 생성
            // 원본 이미지에 BitwiseAnd 연산을 통해 원하는 색상 추출
            // 더 정확한 색상을 RGB로 지정
            // 최솟값 ~ 최댓값 사이의 색상(RGB는 3채널로 3채널 Scalar)을 이용해 mask 생성
            Cv2.InRange(mv1[0], new Scalar(0, 255, 0), new Scalar(10, 255, 0), mask1);
            // -> 이거 두 번째가 Green인데 세 번째 수는 어떻던지 Green 검출 가능... openCV 색상에 문제가 있어보임.
            // -> 2020.07.29 마지막으로 확정. 사진 파일이 어떤 경우로 압축이던 전송되며 일반 색상에 명도가 섞임.
            //      그래서 RGB에서 명도를 담는 R 부분에 10까지만 추가하니 색 정보 전부 불러 읽어옴. 끝!

            // 이렇게도 사용가능.
            // Cv2.InRange(mv1[0], new Scalar((double)Scalar.Green), new Scalar((double)Scalar.Green), mask1);

            // 원래 HSV가 노이즈가 심해서 안쓰려다가 온도부분만 잘려서 온다면 색을 더 잘 읽어서 다시 HSV로 복귀..
            // 하려 했다가 역시나 노이즈 프러블럼. RGB에서 채도 부분 가져오는 것이 낫다. RGB짱.
            //Cv2.CvtColor(src1, src1, ColorConversionCodes.BGR2HSV);
            //mv1 = Cv2.Split(src1);
            //Cv2.CvtColor(src1, src1, ColorConversionCodes.HSV2BGR);
            
            //Cv2.InRange(mv1[0], new Scalar(55, 0, 0), new Scalar(66, 0, 0), mask1);

            //Cv2.BitwiseOr(mask0, mask1, mask1);

            // Cv2.BitwiseAnd(입력1, 입력2, 출력)
            // 원본 이미지는 3채널, mask는 1채널이기에 채널 수 맞추기 위해 3채널로 확장
            Cv2.BitwiseAnd(src1, mask1.CvtColor(ColorConversionCodes.GRAY2BGR), src1);

            // Mat 형식은 pictureBox에 띄울 수 없으므로 다시 Bitmap 형식으로 변환
            bmp1 = BitmapConverter.ToBitmap(src1);

            try
            {
                pictureBox1.Image = bmp1;
            }
            catch { }

            // 온도가 높은 사람의 경우 다른 색으로 표현하기에 두 가지 경로
            // 희한하게 RGB 색공간에서 255, 0, 0은 RED만 검출하지 않고 255, 255, 255인 WHITE까진 검출해버림. HSV도 마찬가지로 R에 W 같이 검출.
            // RGB에선 R에 W, B을 포함하는 듯함. 대신 BGR은 B에 W, B를 포함하는 것으로 보임.
            // RGB로 변환하지 않고 BGR 색공간에서 R을 검출해야 W, B가 섞여나오지 않는 것을 발견.
            // BGR에서 바로 통합하려하면 에러남. BGR2BGR555 또는 BGR565로 유사하게 변환하는 것이 효과적.(근데 BGR555, 565가 뭔지 모르겠음. 565가 좀 더 정확도를 높여줌..)
            // 0, 0, 255 사용 시 RED만 검출하기에 적합.
	// BGR565는 명도 정보가 강해서 노이즈가 심하여 BGR555로 진행
            Cv2.CvtColor(src2, src2, ColorConversionCodes.BGR2BGR555);
            mv2 = Cv2.Split(src2);
            Cv2.CvtColor(src2, src2, ColorConversionCodes.BGR5552BGR);

            // Cv2.InRange(mv2[0], new Scalar(0, 0, 225), new Scalar(3, 3, 255), mask2);
            Cv2.InRange(mv2[0], new Scalar((double)Scalar.Red), new Scalar((double)Scalar.Red), mask2) ;

            Cv2.BitwiseAnd(src2, mask2.CvtColor(ColorConversionCodes.GRAY2BGR), src2);
            bmp2 = BitmapConverter.ToBitmap(src2);

            try
            {
                pictureBox2.Image = bmp2;
            }
            catch { }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            // Button3 클릭 이벤트
            // 특정 색상만 추출한 이미지를 이진화하여 ORC 정확도 향상

            // 기본 이진화 코드(시간이 오래걸림)
            // for(int i=0; i<bmp1.Width; i++)
            // {
            //     for(int j=0; j<bmp1.Height; j++)
            //     {
            //         Color c = bmp1.GetPixel(i, j);
            //         // int binary = (c.R + c.G + c.B) / 3;
            //         // 특정 색상만 추출했기 때문에 나누기 안하는 것이 정확도에 나은듯?
            //         int binary = (c.R + c.G + c.B);
            // 
            //         if (binary > 60)
            //             bmp1.SetPixel(i, j, Color.Black);
            //         else
            //             bmp1.SetPixel(i, j, Color.White);
            //     }
            // }
            // pictureBox1.Image = bmp1;

            // for (int i = 0; i < bmp2.Width; i++)
            // {
            //     for (int j = 0; j < bmp2.Height; j++)
            //     {
            //         Color c = bmp2.GetPixel(i, j);
            //         // int binary = (c.R + c.G + c.B) / 3;
            //         // 특정 색상만 추출했기 때문에 나누기 안하는 것이 정확도에 나은듯?
            //         int binary = (c.R + c.G + c.B);
            // 
            //         if (binary > 60)
            //             bmp2.SetPixel(i, j, Color.Black);
            //         else
            //             bmp2.SetPixel(i, j, Color.White);
            //     }
            // }
            // pictureBox2.Image = bmp2;

            // OpenCV를 이용한 이진화(시간 엄청 단축)
            Mat gray1 = new Mat();
            Mat gray2 = new Mat();

            Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray1, src1, 50, 240, ThresholdTypes.BinaryInv);

            bmp1 = BitmapConverter.ToBitmap(src1);

            pictureBox1.Image = bmp1;

            Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray2, src2, 5, 240, ThresholdTypes.BinaryInv);

            bmp2 = BitmapConverter.ToBitmap(src2);

            pictureBox2.Image = bmp2;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            // Button4 클릭 이벤트
            // Tesseract를 이용하여 문자 인식 후 textBox1에 출력
            // LSTM으로 훈련된 학습 모델과 데이터를 사용

            Pix pix1 = PixConverter.ToPix(bmp1);
            Pix pix2 = PixConverter.ToPix(bmp2);

            // TesseractEngine(데이터 파일 경로, 언어, 엔진 모드);
            
            // 학습 data를 digit으로 바꿨기 때문에 whitelist 불필요
            // 게다가 Tesseract 4.0부터는 whitelist, blacklist 적용 안된다함.
            // whitelist : 여러가지가 학습된 데이터기에 특정 값 위주로 출력하기 위해 사용
            // blacklist : 특정 값을 배제하고 출력
            // string whitelist = "0123456789/";
            // engine.SetVariable("tessdit", whitelist);

            var result1 = engine.Process(pix1);
            textBox1.Text = result1.GetText();
            result1.Dispose();
    
            var result2 = engine.Process(pix2);
            textBox2.Text = result2.GetText();
            result2.Dispose();
            
            // MessageBox.Show(result.GetText());
        }


    }
}
