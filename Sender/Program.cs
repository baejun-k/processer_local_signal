using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Sender
{
	class Program {

		static void Sender() {
			// ReceiveTaskMap으로 할당 된 공유 메모리를 찾는다.
			using(var mmf = MemoryMappedFile.OpenExisting("ReceiveTestMap"))
			// 공유 메모리의 stream
			using(var vst = mmf.CreateViewStream())
			using(var writer = new BinaryWriter(vst)) {
				// ReceiveTest라는 신호를 받는다.
				EventWaitHandle signal = new EventWaitHandle(
					false, EventResetMode.AutoReset, "ReceiveTest");
				// 동기화를 위한 mutex
				Mutex mutex = new Mutex(false, "TestMutex");
				while(true) {
					// Console에 입력한 메시지
					string message = Console.ReadLine();
					// 다른 스레드에서의 공유메모리 사용을 종료되길 기다림
					mutex.WaitOne();
					writer.BaseStream.Position = 0;
					writer.Write(message);
					mutex.ReleaseMutex();
					signal.Set();
					if(message.Equals("@exit"))
						break;
				}
			}
		}

		static void Main(string[] args) {
			Console.WriteLine("Sender Program.\n\n");
			Sender();
		}
	}
}
