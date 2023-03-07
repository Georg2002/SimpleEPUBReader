#pragma once

namespace RenderSupport
{
	void Setup(HWND wHandle, byte* fontData, UINT32 fontDataLength);

	void BeginDraw(float targetAreaX, float targetAreaY, float targetAreaWidth, float targetHeight);

	void EndDraw();
}
