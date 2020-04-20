#include<iostream>
#include <WinSock2.h>
#include<windows.h>
#pragma comment(lib, "WS2_32.lib")
using namespace std;
#pragma warning (disable: 4996)
SOCKET SocketK;

int main() {
	setlocale(LC_ALL, "rus");
	WSADATA data;
	int res = WSAStartup(MAKEWORD(2, 2), &data);
	if (res != 0) {
		cerr << "Don't include library: " << res << endl;
		system("pause");
		return 1;
	}

	SocketK = socket(AF_INET, SOCK_STREAM, NULL);
	if (SocketK == INVALID_SOCKET) {
		cout << "Don't Create Socket: " << WSAGetLastError() << endl;
		WSACleanup();
		system("pause");
		return 2;
	}

	sockaddr_in sock_inK;
	sock_inK.sin_family = AF_INET;
	sock_inK.sin_port = htons(110);
	sock_inK.sin_addr.S_un.S_addr = inet_addr("127.0.0.1");

	int status = connect(SocketK, (sockaddr*)&sock_inK, sizeof(sock_inK));
	if (status == SOCKET_ERROR) {
		cerr << "Don't connectK: " << WSAGetLastError() << endl;
		system("pause");
		return 3;
	}

	char message[30];
	int sizeMsg = 0;
	bool end = false;
	int i = 0;
	cout << "connected!\n";
	do {
		sizeMsg = recv(SocketK, message, sizeof(message), NULL);
		for (i = 0; i < sizeMsg; i++)
			cout << message[i];
	} while (sizeMsg == sizeof(message));
	strcat(message, "\r\n");
	int sizestr = 0;
	int command = 0;
	int number = 0;
	
	do {
		cout << "1.Авторизация.\n2.Просмотр количества сообщений.\n3.Открыть сообщение.\n4.Выход." << endl;
		cin >> command;

		switch (command) {
		case 1:
			char name[100];
			cout << "user ";
			cin >> name;
			strcpy(message, "user ");
			strcat(message, name);
			strcat(message, "\r\n");

			send(SocketK, message, sizeof(message), 0);

			cout << "password: ";
			strcpy(message, "pass ");
			char pass[100];
			cin >> pass;
			strcat(message, pass);
			strcat(message, "\r\n");

			send(SocketK, message, sizeof(message), 0);
			ZeroMemory(message, sizeof(message));
			recv(SocketK, message, sizeof(message), 0);
			cout << message << endl;
			break;
		case 2:
			ZeroMemory(message, sizeof(message));
			strcpy(message, "list");
			strcat(message, "\r\n");
			send(SocketK, message, sizeof(message), 0);
			do {
				ZeroMemory(message, sizeof(message));
				sizeMsg = recv(SocketK, message, sizeof(message), 0);
				if (sizeMsg < sizeof(message)) {
					end = true;
					cout << message;
				}
				else
					for (int i = 0; i < sizeMsg; i++)
						cout << message[i];

			} while (!end);
			end = false;
			break;
		case 3:
			if (strcmp(message, "+OK 0 0\r\n") != 0) {
				ZeroMemory(message, sizeof(message));
				char number[10];
				cout << "Введите номер сообщения: ";
				cin >> number;
				strcpy(message, "retr ");
				strcat(message, number);
				strcat(message, "\r\n");
				send(SocketK, message, sizeof(message), 0);
				do {
					ZeroMemory(message, sizeof(message));
					sizeMsg = recv(SocketK, message, sizeof(message), 0);
					if (sizeMsg < sizeof(message)) {
						end = true;
						cout << message;
					}
					else
						for (int i = 0; i < sizeMsg; i++)
							cout << message[i];

				} while (!end);
				end = false;
			}
			else cout << "Сообщений нет" << endl;
			break;
		case 4:
			strcpy(message, "quit\r\n");
			send(SocketK, message, sizeof(message), 0);
			ZeroMemory(message, sizeof(message));
			recv(SocketK, message, sizeof(message), 0);
			cout << message;
			break;
		default:
			cout << "Неверная команда" << endl;
			break;
		}
	} while (command != 4);

	closesocket(SocketK);
	WSACleanup();
	system("pause");
	return 0;
}