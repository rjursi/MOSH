#include "server_socket_class.h"
#include <time.h>

server_socket_class::server_socket_class() {
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	sock = socket(PF_INET, SOCK_DGRAM, IPPROTO_UDP);

	setsockopt(sock, SOL_SOCKET, SO_BROADCAST, (char*)&broadcast_enable, sizeof(broadcast_enable));

	memset(&sock_addr, 0, sizeof(sock_addr));
	sock_addr.sin_family = AF_INET;
	sock_addr.sin_addr.s_addr =  htonl(INADDR_BROADCAST); // inet_addr("192.168.31.200");
	sock_addr.sin_port = htons(PORT);
}

server_socket_class::~server_socket_class() {
	closesocket(sock);
	WSACleanup();
}

void server_socket_class::connect_client() {
	sendto(sock, (char*)&var_request, sizeof(var_request), 0, (const SOCKADDR*)&sock_addr, sizeof(sock_addr));

	ZeroMemory(var_response, sizeof(var_response));
	recvfrom(sock, var_response, sizeof(var_response), 0, (SOCKADDR*)&recver_addr, &recver_addr_size);
}

void server_socket_class::sendfile() {
	clock_t start, end;

	// 파일 열기
	filepointer = fopen("ScreenCapture.jpeg", "rb");
	fseek(filepointer, 0, SEEK_END);
	file_size = ftell(filepointer);
	// 파일 포인터를 제일 앞으로 이동
	rewind(filepointer);

	sendto(sock, (char*)&file_size, sizeof(file_size), 0, (const SOCKADDR*)&recver_addr, sizeof(recver_addr));
	cout << "파일 크기 : " << file_size << " Byte" << endl;

	cout << "\nUploading ";

	start = clock();
	// 전송 루프
	buf = new char[BUFSIZE];
	while (1) {
		ZeroMemory(buf, _msize(buf));
		send_size = fread(buf, 1, _msize(buf), filepointer);
		sendto(sock, buf, _msize(buf), 0, (SOCKADDR*)&recver_addr, sizeof(recver_addr));

		// 응답 수신
		recvfrom(sock, msgbuf, sizeof(msgbuf), 0, NULL, 0);
		cout << msgbuf << " ";

		file_size -= send_size;
		if (file_size == 0) break;

		// 나머지 전송
		if (file_size < BUFSIZE) {
			delete[] buf;
			buf = NULL;
			buf = new char[file_size];
		}
	}
	end = clock();
	fclose(filepointer);

	delete[] buf;
	buf = NULL;

	// 전송 결과
	cout << "\n\n파일전송 성공 (Time : " << (double)(end - start) << ")\n" << endl;
}