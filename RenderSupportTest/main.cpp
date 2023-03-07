#include "pch.h"
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
#pragma comment(lib, "windowscodecs.lib")

using namespace std;

ID2D1Factory* d2d;
ID2D1HwndRenderTarget* hwndRt;
HRESULT hr;
LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
IDWriteFontFace* fontFace;
IDWriteFontFace5* fallbackFontFace;
IDWriteFactory7* writeFactory;

int main()
{
	wWinMain(nullptr, nullptr, nullptr, 1);
}


int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
	hr = CoInitialize(nullptr);
	hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &d2d);


	char* fontData;

	ifstream file("D:\\Informatik\\EPUBReader\\EPUBRenderer\\Fonts\\Hiragino-Sans-GB-W3.otf", ios::in | ios::binary | ios::ate);
	streampos size;
	if (file.is_open())
	{
		size = file.tellg();
		fontData = (char*)calloc(size, 1);
		file.seekg(0, ios::beg);
		file.read(fontData, size);
		file.close();
		printf("the entire file content is in memory");
	}



	IDWriteFontFile* fontFile;
	IDWriteFontFileLoader* fontFileLoader;
	IDWriteFont* writeFont;
	IDWriteInMemoryFontFileLoader* inMemoryFontFileLoader;


	hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory7), reinterpret_cast<IUnknown**>(&writeFactory));
	hr = writeFactory->CreateInMemoryFontFileLoader(&inMemoryFontFileLoader);
	hr = writeFactory->RegisterFontFileLoader(inMemoryFontFileLoader);
	hr = inMemoryFontFileLoader->CreateInMemoryFontFileReference(writeFactory, fontData, (UINT32)size, nullptr, &fontFile);
	//hr = fontSetBuilder->AddFontFile(fontFile);

	hr = writeFactory->CreateFontFace(DWRITE_FONT_FACE_TYPE_UNKNOWN, 1, &fontFile, 0, DWRITE_FONT_SIMULATIONS_NONE, &fontFace);


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

	// window creation stuff
	WNDCLASSEX wcex{
	sizeof(WNDCLASSEX)
	};
	wcex.lpfnWndProc = WndProc;
	wcex.hInstance = hInstance;
	wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wcex.lpszClassName = L"MainWindow";
	RegisterClassEx(&wcex);

	HWND hWnd = CreateWindow(wcex.lpszClassName, L"Direct2D", WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, 0, 1000, 1000, nullptr, nullptr, hInstance, nullptr);

	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);

	for (MSG msg; GetMessage(&msg, nullptr, 0, 0);) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}
}

ID2D1SolidColorBrush* brushBlue;
ID2D1SolidColorBrush* brushRed;
ID2D1SolidColorBrush* brushBlack;
D2D1_COLOR_F white;
D2D1_SIZE_U windowSize;
long count = 0;

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message) {
	case WM_CREATE:
		// create the HWND render target for this window
		d2d->CreateHwndRenderTarget(
			D2D1::RenderTargetProperties(),
			D2D1::HwndRenderTargetProperties(hWnd, D2D1::SizeU(1000, 1000)),
			&hwndRt);
		hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Blue, 1), &brushBlue);
		hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, 1), &brushRed);
		hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Black, 1), &brushBlack);
		white.a = white.b = white.g = white.r = 1;
		hwndRt->SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);
		break;
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		DWRITE_GLYPH_RUN run;
		{
			float size = 15;
			run.glyphAdvances = &size;
			run.fontEmSize = size;
			run.fontFace = fontFace;
			run.glyphCount = 1;
			run.bidiLevel = 0;
			run.isSideways = false;
		}

		D2D1_POINT_2F drawPos;

		DWRITE_GLYPH_OFFSET gO;
		gO.advanceOffset = 0;
		gO.ascenderOffset = 0;
		run.glyphOffsets = &gO;

		hwndRt->BeginDraw();

		hwndRt->Clear(white);
		//hwndRt->PushAxisAlignedClip(D2D1::RectF(200, 300, 700,900), D2D1_ANTIALIAS_MODE_ALIASED);
		run.fontFace = fontFace;

		auto width = windowSize.width - 20;

		int charPerRow = width / 15;

		for (size_t i = 0; i < 8000; i++)
		{
			int row = i / charPerRow;
			drawPos.x = (i % charPerRow) * 15 + 20;
			drawPos.y = row * 15 + 20;
			UINT character = L'あ' + i;//あ
			UINT16 gi = 0;
			fontFace->GetGlyphIndices((unsigned int*)&character, 1, &gi);
			if (gi == 0)
			{
				character = L'X';
				fontFace->GetGlyphIndices((unsigned int*)&character, 1, &gi);
			}
			hwndRt->DrawGlyphRun(drawPos, &run, brushBlack);
		}

		//hwndRt->PopAxisAlignedClip();
		hr = hwndRt->EndDraw();

		EndPaint(hWnd, &ps);
	}


	break;

	case WM_DESTROY:
		PostQuitMessage(0);
		break;

	case WM_SIZE:
		RECT rc;
		GetClientRect(hWnd, &rc);
		windowSize = D2D1::SizeU(rc.right, rc.bottom);
		hwndRt->Resize(windowSize);
		InvalidateRect(hWnd, &rc, false);

		break;

	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}

	return 0;
}
