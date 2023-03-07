#include "pch.h"
#include "Class1.h"
#include <windows.h>
#include <wincodec.h>
#include <d2d1_1.h>
#include <atlbase.h>
#include <vector>

#include <dwrite_3.h>
#include <stdlib.h>

#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "windowscodecs.lib")

using namespace std;


using namespace RenderSupport;

ID2D1Factory* d2d;
ID2D1HwndRenderTarget* hwndRt;
HWND windowHandle;
HRESULT hr;
LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
IDWriteFontFace* fontFace;
IDWriteFontFace5* fallbackFontFace;
IDWriteFactory7* writeFactory;

ID2D1SolidColorBrush* brushBlue;
ID2D1SolidColorBrush* brushRed;
ID2D1SolidColorBrush* brushBlack;
D2D1_COLOR_F white;
D2D1_SIZE_U windowSize;
long count = 0;


void RenderSupport::Setup(HWND wHandle, byte* fontData, UINT32 fontDataLength)
{
	windowHandle = wHandle;
	IDWriteFontFile* fontFile;
	IDWriteFontFileLoader* fontFileLoader;
	IDWriteFont* writeFont;
	IDWriteInMemoryFontFileLoader* inMemoryFontFileLoader;


	hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory7), reinterpret_cast<IUnknown**>(&writeFactory));
	hr = writeFactory->CreateInMemoryFontFileLoader(&inMemoryFontFileLoader);
	hr = writeFactory->RegisterFontFileLoader(inMemoryFontFileLoader);
	hr = inMemoryFontFileLoader->CreateInMemoryFontFileReference(writeFactory, fontData, fontDataLength, nullptr, &fontFile);
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




	d2d->CreateHwndRenderTarget(
		D2D1::RenderTargetProperties(),
		D2D1::HwndRenderTargetProperties(windowHandle, D2D1::SizeU(1000, 1000)),
		&hwndRt);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Blue, 1), &brushBlue);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, 1), &brushRed);
	hwndRt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Black, 1), &brushBlack);
	white.a = white.b = white.g = white.r = 1;
	hwndRt->SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE);
}


void RenderSupport::BeginDraw(float targetAreaX, float targetAreaY, float targetAreaWidth, float targetHeight)
{
	hwndRt->PushAxisAlignedClip(D2D1::RectF(200, 300, 700, 900), D2D1_ANTIALIAS_MODE_ALIASED);
	PAINTSTRUCT ps;
	HDC hdc = BeginPaint(windowHandle, &ps);
	hwndRt->BeginDraw();
	hwndRt->Clear(white);
}

void RenderSupport::EndDraw()
{

	hwndRt->PopAxisAlignedClip();
	InvalidateRect(windowHandle, &targetArea, false);
}