#include<iostream>
#include <WinSock2.h>
#include<windows.h>
#include <fstream>
#include<vector>
#include <string>
#pragma warning (disable: 4996)
#pragma comment(lib, "WS2_32.lib")
using namespace std;

SOCKET client;

int main() {
	setlocale(LC_ALL, "rus");
	WSADATA data;
	int res = WSAStartup(MAKEWORD(2, 2), &data);
	if (res != 0) {
		cerr << "Don't include library: " << res << endl;
		system("pause");
		return 1;
	}

	client = socket(AF_INET, SOCK_STREAM, NULL);
	if (client == INVALID_SOCKET) {
		cout << "Don't Create Socket: " << WSAGetLastError() << endl;
		WSACleanup();
		system("pause");
		return 2;
	}

	sockaddr_in sock_inK;
	sock_inK.sin_family = AF_INET;
	sock_inK.sin_port = htons(21);
	sock_inK.sin_addr.S_un.S_addr = inet_addr("127.0.0.1");

	int status = connect(client, (sockaddr*)&sock_inK, sizeof(sock_inK));
	if (status == SOCKET_ERROR) {
		cerr << "Don't connectK: " << WSAGetLastError() << endl;
		system("pause");
		return 3;
	}

	bool end = false;
	ifstream in;
	in.open("users.txt");
	if (!in.is_open()) {
		cout << "ERROR users " << GetLastError() << endl;
		system("pause");
		return 0;
	}
	string s;
	vector<string> v;
	char user[100];
	char pass[100];
	char message[100];
	int i = 0, j = 0;

	ZeroMemory(message, sizeof(message));
	recv(client, message, sizeof(message), 0);
	cout << message << endl;
	
	while (getline(in, s)) {
		v.push_back(s);
	}
	char mas[100];
	int n;
	do {
		n = 0;
		ZeroMemory(mas, sizeof(mas));
		ZeroMemory(user, sizeof(user));
		ZeroMemory(pass, sizeof(pass));

		strcpy(user, "user ");
		
			for (j; j < v[i].size(); j++) {
				if (v[i][j] == ':') break;
				mas[n] = v[i][j];
				n++;
			}
			j++;
		strcat(user, mas);
		strcat(user, "\r\n");
		send(client, user, sizeof(user), 0);
		recv(client, message, sizeof(message), 0);

		ZeroMemory(message, sizeof(message));
		ZeroMemory(mas, sizeof(mas));
		n = 0;
		strcpy(pass, "pass ");
		
			for (j; j < v[i].size(); j++) {
				mas[n] = v[i][j];	
				n++;
			}
			
		strcat(pass, mas);
		strcat(pass, "\r\n");
		send(client, pass, sizeof(pass), 0);
		recv(client, message, sizeof(message), 0);
		if (strcmp(message, "230 Logged in successfully\r\n") == 0)
			cout << v[i] << " - EXIST" << endl;
		else cout << v[i] << " - NOT EXIST" << endl;
		j = 0;
		i++;
		if (i == v.size())end = true;
	} while (!end);

	closesocket(client);
	WSACleanup();
	system("pause");
	return 0;
}