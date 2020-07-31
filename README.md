# T_OCR
- Used for recognizing temperature on the screen from thermal camera.
- 열화상 카메라로 촬영하여 표시된 온도를 화면에서 읽는 기능.
- 이미지상의 광학 문자를 전처리로 opencv를 사용하여 특정 색 추출 후 OCR

# 참고 자료
1. https://076923.github.io/posts/C-tesseract-2/
2. https://shalchoong.tistory.com/8

# 사용 Data
- https://github.com/Shreeshrii/tessdata_shreetest
- 데이터 셋 중, digits.traindata 사용

# 사용 방법
1. Using specific color from the image
이미지에서 특정 색을 기준으로 뽑아온 후, 이진화를 통해 전처리를 진행하므로 코드의
- Cv2.CvtColor(src1, src1, ColorConversionCodes.BGR2RGB);
- mv1 = Cv2.Split(src1);
- Cv2.CvtColor(src1, src1, ColorConversionCodes.RGB2BGR);

- Cv2.InRange(mv1[0], new Scalar(0, 255, 0), new Scalar(14, 255, 3), mask1);

이 부분들을 사용할 색을 검출 후 수정하여 사용하면 됨.
- !!주의점.
- BGR, RGB 둘 다 첫 채널에 명도(?)가 포함되어 BGR채널에서는 Blue, RGB채널에선 Red의 검출이 불가능.
- RGB, BGR, HSV 등을 사용하여 원하는 특정 색을 추출할 것.

2. Train data sets
TesseractEngine engine = new TesseractEngine(@"./tessdata", "digits", EngineMode.Default);
위 코드에서 테스트 데이터를 불러오므로 프로젝트 파일의 Debug 파일이 위치한 곳에 tessdata라는 폴더를 생성 후,
digits.traindata 파일을 넣으면 됨.

- 첫 기능 구현으로 부족한 부분이 많아 추후 수정할 예정. 특정 색을 통해 전처리가 잘 되었다면 ORC Accuracy는 90% 이상.
