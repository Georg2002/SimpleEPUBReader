#include <windows.h>
#include <wincodec.h>
#include <d2d1_1.h>
#include <atlbase.h>
#include <vector>

#include <dwrite_3.h>
#include <iostream>
#include <fstream>
#include <stdlib.h>

#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "Dwrite")
#pragma comment(lib, "windowscodecs.lib")

using namespace std;


ID2D1Factory* d2d;
ID2D1HwndRenderTarget* hwndRt;
HRESULT hr;

IDWriteFontFace* fontFace;
IDWriteFontFace* boldFontFace;
IDWriteFontFace5* fallbackFontFace;
IDWriteFontFace5* boldFallbackFontFace;

IDWriteFactory7* writeFactory;

ID2D1SolidColorBrush* brushBlack;
ID2D1SolidColorBrush* brushRed;

ID2D1SolidColorBrush* selectionBrush;

ID2D1SolidColorBrush* markingBrushes[5];


D2D1_COLOR_F white;
D2D1_SIZE_U windowSize;
long count = 0;

char* fontData;
UINT32 fontDataSize;

DWRITE_GLYPH_RUN run;

LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);

HWND windowHandle;
HWND parentHWnd;


PAINTSTRUCT ps;


void Setup(HWND wHandle, char* fontData, UINT32 fontDataLength);

extern "C"
{
	__declspec(dllexport) HWND SetWindow(HWND parentWindow)
	{
		parentHWnd = parentWindow;
		auto style = GetWindowLongW(parentWindow, GWL_STYLE);
		SetWindowLongW(parentWindow, GWL_STYLE, style ^ WS_CLIPCHILDREN);
		wWinMain(nullptr, nullptr, nullptr, 1);
		return windowHandle;
	}

	__declspec(dllexport) void BeginDraw()
	{
		HDC hdc = BeginPaint(windowHandle, &ps);
		hwndRt->BeginDraw();
		hwndRt->Clear(white);
	}

	__declspec(dllexport) void EndDraw()
	{
		auto state = hwndRt->CheckWindowState();
		hr = hwndRt->EndDraw();
		EndPaint(windowHandle, &ps);
	}


	__declspec(dllexport) void DestroyW()
	{
		DestroyWindow(windowHandle);
	}

	__declspec(dllexport) void HandleMessages(UINT msg, WPARAM wparam, LPARAM lparam)
	{
		WndProc(windowHandle, msg, wparam, lparam);
	}

	__declspec(dllexport) void DrawCharacter(float x, float y, UINT character, float size, bool bold, float rotation);
}

int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
	//hr = CoInitialize(nullptr);

	ifstream file("D:\\Informatik\\EPUBReader\\EPUBRenderer\\Fonts\\Hiragino-Sans-GB-W3.otf", ios::in | ios::binary | ios::ate);
	streampos size;
	if (file.is_open())
	{
		size = file.tellg();
		fontData = (char*)malloc(size);
		fontDataSize = size;
		file.seekg(0, ios::beg);
		file.read(fontData, size);
		file.close();
	}


	// window creation stuff
	WNDCLASSEX wcex{
	sizeof(WNDCLASSEX)
	};
	wcex.lpfnWndProc = WndProc;
	wcex.hInstance = hInstance;
	wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wcex.lpszClassName = L"EPUBTextRenderer";

	RegisterClassEx(&wcex);

	HWND hWnd = CreateWindow(wcex.lpszClassName, L"Direct2D", WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, 0, 1000, 1000, parentHWnd, nullptr, hInstance, nullptr);
	windowHandle = hWnd;
	SetParent(hWnd, parentHWnd);
	auto style = GetWindowLongW(hWnd, GWL_STYLE);
	SetWindowLongW(hWnd, GWL_STYLE, WS_CHILD | WS_VISIBLE | WS_CHILDWINDOW | WS_CLIPCHILDREN | style & !(WS_BORDER | WS_CAPTION | WS_DLGFRAME | WS_TILED));
	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);

	/*for (MSG msg; GetMessage(&msg, windowHandle, 0, WM_WINDOWPOSCHANGED);) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}*/
}



void Setup(HWND wHandle, char* fontData, UINT32 fontDataLength)
{
	windowHandle = wHandle;
	IDWriteFontFile* fontFile;
	IDWriteInMemoryFontFileLoader* inMemoryFontFileLoader;

	hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &d2d);
	hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory7), reinterpret_cast<IUnknown**>(&writeFactory));
	hr = writeFactory->CreateInMemoryFontFileLoader(&inMemoryFontFileLoader);
	hr = writeFactory->RegisterFontFileLoader(inMemoryFontFileLoader);
	hr = inMemoryFontFileLoader->CreateInMemoryFontFileReference(writeFactory, fontData, fontDataLength, nullptr, &fontFile);

	//hr = fontSetBuilder->AddFontFile(fontFile);

	hr = writeFactory->CreateFontFace(DWRITE_FONT_FACE_TYPE_UNKNOWN, 1, &fontFile, 0, DWRITE_FONT_SIMULATIONS_NONE, &fontFace);
	hr = writeFactory->CreateFontFace(DWRITE_FONT_FACE_TYPE_UNKNOWN, 1, &fontFile, 0, DWRITE_FONT_SIMULATIONS_BOLD, &boldFontFace);


	IDWriteFontCollection3* sysFontCollection;
	hr = writeFactory->GetSystemFontCollection((IDWriteFontCollection**)&sysFontCollection);
	UINT32 fontIndex = 0;
	BOOL exists = false;
	hr = sysFontCollection->FindFamilyName(L"Arial", &fontIndex, &exists);
	IDWriteFontFamily2* fallbackFontFamily;
	hr = sysFontCollection->GetFontFamily(fontIndex, &fallbackFontFamily);
	IDWriteFont3* fallbackFont;
	hr = fallbackFontFamily->GetFirstMatchingFont(DWRITE_FONT_WEIGHT_NORMAL, DWRITE_FONT_STRETCH_NORMAL, DWRITE_FONT_STYLE_NORMAL, (IDWriteFont**)&fallbackFont);

	//throws nullptr
	//hr = fallbackFont->CreateFontFace((IDWriteFontFace3**)fallbackFontFace);




	d2d->CreateHwndRenderTarget(
		D2D1::RenderTargetProperties(),
		D2D1::HwndRenderTargetProperties(windowHandle, D2D1::SizeU(1000, 1000)),
		&hwndRt);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Black, 1), &brushBlack);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, 1), &brushRed);
	white.a = white.b = white.g = white.r = 1;
	hwndRt->SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);

	// DictSelectionColor = new SolidColorBrush(Color.FromArgb(100, 50, 50, 50));
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(100.0f / 255.0f, 50.0f / 255.0f, 50.0f / 255.0f, 50.0f / 255.0f), &selectionBrush);

	float alpha = 100.0f / 255.0f;
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, alpha), &markingBrushes[1]);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, alpha), &markingBrushes[2]);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Yellow, alpha), &markingBrushes[3]);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Blue, alpha), &markingBrushes[4]);

	/*
	new SolidColorBrush(new Color(){ R = 255, G = 0, B = 0, A = Alpha }),
	new SolidColorBrush(new Color(){ R = 0, G = 255,B = 0,A = Alpha }),
	new SolidColorBrush(new Color(){ R = 255, G = 255,B = 0,A = Alpha }),
	new SolidColorBrush(new Color(){ R = 0, G = 0,B = 255,A = Alpha })
	*/

	run.fontFace = fontFace;
	run.glyphCount = 1;
	run.bidiLevel = 0;
	run.isSideways = false;
}


D2D1_POINT_2F drawPos;
DWRITE_GLYPH_OFFSET gO;

void DrawCharacter(float x, float y, UINT character, float size, bool bold, float rotation)
{
	//point is top middle
	gO.advanceOffset = 0;
	gO.ascenderOffset = 0;
	run.glyphOffsets = &gO;

	run.glyphAdvances = &size;
	run.fontEmSize = size;

	UINT16 gi = 0;
	fontFace->GetGlyphIndices((unsigned int*)&character, 1, &gi);
	if (gi == 0)
	{
		character = L'X';
		fontFace->GetGlyphIndices((unsigned int*)&character, 1, &gi);
		run.fontFace = bold ? boldFallbackFontFace : fallbackFontFace;
	}
	else run.fontFace = bold ? boldFontFace : fontFace;
	DWRITE_FONT_METRICS metrics;
	fontFace->GetMetrics(&metrics);

	DWRITE_GLYPH_METRICS glyphMetrics;
	fontFace->GetDesignGlyphMetrics(&gi, 1, &glyphMetrics);

	float scale = size / (float)metrics.designUnitsPerEm;
	drawPos.x = x - (float)glyphMetrics.advanceWidth * scale / 2;//(size - (float)glyphMetrics.advanceWidth * scale) / 2;
	drawPos.y = y + ((float)metrics.capHeight * scale + size) / 2;//position in middle

	run.glyphIndices = &gi;
	if (rotation != 0)	hwndRt->SetTransform(D2D1::Matrix3x2F::Rotation(rotation, D2D1::Point2F(x + size / 2, y + size / 2)));
	hwndRt->DrawGlyphRun(drawPos, &run, brushBlack);
	if (rotation != 0) hwndRt->SetTransform(D2D1::Matrix3x2F::Identity());



	D2D_POINT_2F p1 = { x - 4,y };
	D2D_POINT_2F p2 = { x + 4,y };
	hwndRt->DrawLine(p1, p2, brushRed, 1);
	p1 = { x,y - 4 };
	p2 = { x,y + 4 };
	hwndRt->DrawLine(p1, p2, brushRed, 1);


}

extern "C" __declspec(dllexport) void DrawMarkingRect(bool isMarked, int colorIndex, bool isSelected, float x0, float y0, float x1, float y1)
{
	if (isMarked) hwndRt->FillRectangle(D2D1::RectF(x0, y0, x1, y1), markingBrushes[colorIndex]);
	if (isSelected) hwndRt->FillRectangle(D2D1::RectF(x0, y0, x1, y1), selectionBrush);

	//      if (letter.MarkingColorIndex != 0) drawingContext.DrawRectangle(MarkingColors[letter.MarkingColorIndex], null, Rect);
		  //      if (letter.DictSelected && !letter.IsRuby) drawingContext.DrawRectangle(Letter.DictSelectionColor, null, Rect);
}


extern "C" __declspec(dllexport) void drawMissingImage(float x0, float y0, float x1, float y1)
{
	auto rect = D2D_RECT_F();
	rect.bottom = y1;
	rect.top = y0;
	rect.left = x0;
	rect.right = x1;

	hwndRt->FillRectangle(rect, brushRed);
}

extern "C" __declspec(dllexport) void drawImage(unsigned char* imageData, int imageSize, float x0, float y0, float x1, float y1)
{
	IWICImagingFactory* pWICFactory = nullptr;
	IWICStream* pStream = nullptr;
	IWICBitmapDecoder* pDecoder = nullptr;
	IWICBitmapFrameDecode* pFrame = nullptr;
	IWICFormatConverter* pConverter = nullptr;
	HRESULT hr = CoInitialize(nullptr); // Initialize COM library

	if (SUCCEEDED(hr))
	{
		// Create WIC Imaging Factory
		hr = CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pWICFactory));
	}

	if (SUCCEEDED(hr))
	{
		// Create a WIC stream to load the byte array
		hr = pWICFactory->CreateStream(&pStream);
	}

	if (SUCCEEDED(hr))
	{
		// Initialize the stream with the byte array data
		hr = pStream->InitializeFromMemory(imageData, imageSize);
	}

	if (SUCCEEDED(hr))
	{
		// Create a decoder for the image from the stream
		hr = pWICFactory->CreateDecoderFromStream(pStream, nullptr, WICDecodeMetadataCacheOnDemand, &pDecoder);
	}

	if (SUCCEEDED(hr))
	{
		// Get the first frame of the image
		hr = pDecoder->GetFrame(0, &pFrame);
	}

	if (SUCCEEDED(hr))
	{
		// Convert the image format to 32bppPBGRA (compatible with Direct2D)
		hr = pWICFactory->CreateFormatConverter(&pConverter);
	}

	if (SUCCEEDED(hr))
	{
		hr = pConverter->Initialize(
			pFrame,
			GUID_WICPixelFormat32bppPBGRA,
			WICBitmapDitherTypeNone,
			nullptr,
			0.f,
			WICBitmapPaletteTypeCustom
		);
	}

	if (SUCCEEDED(hr))
	{
		ID2D1Bitmap* pBitmap;
		// Create a Direct2D bitmap from the WIC bitmap
		hr = hwndRt->CreateBitmapFromWicBitmap(pConverter, nullptr, &pBitmap);

		hwndRt->DrawBitmap(
			pBitmap,
			D2D1::RectF(x0, y0, x1, y1)
		);
	}
}

TRACKMOUSEEVENT mouseevent;

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message) {
	case WM_CREATE:
	{
		// create the HWND render target for this window
		Setup(hWnd, (char*)fontData, fontDataSize);

		mouseevent.cbSize = sizeof(mouseevent);
		mouseevent.dwFlags = TME_LEAVE;
		mouseevent.hwndTrack = hWnd;
		mouseevent.dwHoverTime = 1;

	}
	break;
	case WM_PAINT:
	{
		//BeginDraw();
		/*		wchar_t sampleText[] = L"これもテストです。心の底から事件以外の何物でも無い、難しい漢字使っても善いぞ!。憂鬱とか。";
				int fs = 25;
				int rows = (windowSize.height - 20) / fs;
				for (size_t i = 0; i < 2000; i++)
				{
					DrawCharacter(windowSize.width - fs - 10 - fs * (int)(i / rows), 10 + fs * (i % rows), sampleText[i % 45], fs, false, i % 360);

				}*/
				//EndDraw();
	}


	break;

	case WM_DESTROY:
		PostQuitMessage(0);
		break;

	case WM_MOVE:
	{

		RECT rc;
		GetClientRect(parentHWnd, &rc);
		InvalidateRect(hWnd, &rc, false);
		break;
	}
	case WM_SIZE:
	{
		RECT rc;

		GetClientRect(hWnd, &rc);
		windowSize = D2D1::SizeU(rc.right, rc.bottom);
		hwndRt->Resize(windowSize);
		GetClientRect(parentHWnd, &rc);
		InvalidateRect(hWnd, &rc, false);
	}

	break;
	case WM_MOUSELEAVE:
		break;
	case WM_MOUSEFIRST:
		TrackMouseEvent(&mouseevent);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}

	return 0;
}
