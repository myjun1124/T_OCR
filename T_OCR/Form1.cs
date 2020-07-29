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

namespace T_OCR
{
    public partial class Form1 : Form
    {

        Bitmap bmp1, bmp2;
        Mat src1, src2;
        
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

            // 특정 색 검출을 위해 RBG 변환
            Cv2.CvtColor(src1, src1, ColorConversionCodes.BGR2RGB);
            mv1 = Cv2.Split(src1);
            Cv2.CvtColor(src1, src1, ColorConversionCodes.RGB2BGR);

            // 색상 추출 시 필요한 값을 기준으로 영역 내 색상을 나타내는 마스크 생성
            // 최솟값 ~ 최댓값 사이의 색상(RGB는 3채널로 3채널 Scalar)을 이용해 mask 생성
            Cv2.InRange(mv1[0], new Scalar(0, 240, 0), new Scalar(14, 255, 3), mask1);
            // 원본 이미지에 BitwiseAnd 연산을 통해 원하는 색상 추출
            Cv2.BitwiseAnd(src1, mask1.CvtColor(ColorConversionCodes.GRAY2BGR), src1);

            // 적색은 BGR로 색 추출하는 것이 나음.
            Cv2.CvtColor(src2, src2, ColorConversionCodes.BGR2BGR555);
            mv2 = Cv2.Split(src2);
            Cv2.CvtColor(src2, src2, ColorConversionCodes.BGR5552BGR);

            Cv2.InRange(mv2[0], new Scalar(0, 0, 250), new Scalar(3, 3, 255), mask2);
            // Cv2.InRange(mv2[0], new Scalar((double)Scalar.Red), new Scalar((double)Scalar.Red), mask2);
            Cv2.BitwiseAnd(src2, mask2.CvtColor(ColorConversionCodes.GRAY2BGR), src2);

            // 추출한 색을 바탕으로 이진화.
            Mat gray1 = new Mat();
            Mat gray2 = new Mat();

            Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray1, src1, 50, 240, ThresholdTypes.BinaryInv);


            // Mat 형식은 pictureBox에 띄울 수 없으므로 다시 Bitmap 형식으로 변환
            bmp1 = BitmapConverter.ToBitmap(src1);

            pictureBox1.Image = bmp1;

            Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray2, src2, 50, 240, ThresholdTypes.BinaryInv);

            bmp2 = BitmapConverter.ToBitmap(src2);

            pictureBox2.Image = bmp2;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            // Button4 클릭 이벤트
            // Tesseract를 이용하여 문자 인식 후 textBox1에 출력
            // LSTM으로 훈련된 학습 모델과 데이터를 사용

            Pix pix1 = PixConverter.ToPix(bmp1);
            Pix pix2 = PixConverter.ToPix(bmp2);

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
