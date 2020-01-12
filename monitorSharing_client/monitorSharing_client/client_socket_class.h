#ifndef CLIENTSOCKETCLASS_H
#define CLIENTSOCKETCLASS_H

#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include <string>

#pragma	comment(lib, "ws2_32")

#define BUFSIZE 65000
#define PORT 9000

using namespace std;

class client_socket_class {
private:
	int temp;

	// 소켓 관련 변수
	WSADATA wsaData;
	SOCKET sock;
	SOCKADDR_IN sock_addr;
	// 자기 자신에 대한 소켓 정보를 저장하고 있음
	
	IP_MREQ join_addr;
	// 멀티캐스트 그룹 관련한 정보를 포함하고 있는 구조체

	 
	

	//BOOL broadcast_enable = TRUE;

	// 일대일 연결을 위한 변수
	SOCKADDR_IN sender_addr;
	int sender_addr_size = sizeof(sender_addr);
	int sock_addr_size = sizeof(sock_addr);
	char var_connect[2];
	
	// 파일 데이터 전송에 사용할 변수
	char *totalbuf;
	char *buf;

	string serverIPAddress;
	// 서버 IP 주소


	FILE *filepointer;

	int file_size;
	int recv_size = 0;
	int total_size;

	// 응답 변수
	char msgbuf[2] = ".";

	

public:
	//생성자
	client_socket_class();
	//소멸자
	~client_socket_class();

	// 서버 연결
	void connect_server();

	//파일 수신
	void recvfile();

	void getServerIP();
};

#endif // !CLIENTSOCKETCLASS_H