using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Receiver
{
	class Program {
		static void Receiver() {
			// 공유 메모리 1024바이트 할당
			using(var mmf = MemoryMappedFile.CreateNew("ReceiveTestMap", 1024))
			// 공유 메모리의 Stream을 얻는다.
			using(var vst = mmf.CreateViewStream())
			using(var reader = new BinaryReader(vst)) {
				// ReceiveTest라는 신호를 만든다.
				EventWaitHandle signal = new EventWaitHandle(
					false, EventResetMode.AutoReset, "ReceiveTest");
				// 동기화를 위한 mutex
				Mutex mutex = new Mutex(false, "TestMutex");

				while(true) {
					// 신호가 오기(누군가 메시지를 보내길)를 기다린다.
					signal.WaitOne();
					// 다른 곳에서의 stream 사용이 종료되기를 기다 림
					mutex.WaitOne();
					reader.BaseStream.Position = 0;
					// 공유 메모리에 적힌 값을 string으로 변환
					string message = reader.ReadString();
					// stream 작업이 끝났음을 알림
					mutex.ReleaseMutex();
					// 받은 메시지 데이터 처리
					Console.WriteLine("Received message : " + message);
					if(message.Equals("@exit"))
						break;
				}

			}
		}

		static void Main(string[] args) {
			Console.WriteLine("Receiver Program.\n\n");
			Receiver();
		}
	}
}
