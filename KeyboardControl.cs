using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Poincare.Geometry;

namespace Poincare.Application {
	public class KeyboardControl {
		public SunflowerWindow SunflowerWindow { get; private set; }

		public KeyboardControl(SunflowerWindow poincareWindow) {
			SunflowerWindow = poincareWindow;
	
			SunflowerWindow.Keyboard.KeyDown += Keyboard_KeyDown;
			SunflowerWindow.Keyboard.KeyRepeat = true;
		}

		private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e) {
			bool isShift = SunflowerWindow.Keyboard[Key.LShift] || SunflowerWindow.Keyboard[Key.RShift];
		
			switch (e.Key) {

			case Key.Number1:
			case Key.Keypad1:
				SunflowerWindow.Actors[0].Modulo += isShift ? -1 : 1;
				for (int i = 0; i < 1024; i++)
					new FeedbackActor(i, new Color4(0f, 0f, 0f, 1f), 4);

			//	SunflowerWindow.Cells SunflowerWindow.Actors[0].Modulo

				new FeedbackActor(SunflowerWindow.Actors[0].Modulo, new Color4(1f, 0.5f, 0.5f, 1f), 8);
				if (ModuloActor.FibonacciNumbers.Contains(SunflowerWindow.Actors[0].Modulo)) 
					ModuloActor.AnnounceFibonaccis();

				SunflowerWindow.WriteStatus();
				break;
			
			case Key.Number2:
			case Key.Keypad2:
				SunflowerWindow.Actors[1].Modulo += isShift ? -1 : 1;
				new FeedbackActor(SunflowerWindow.Actors[1].Modulo, new Color4(0f, 1f, 0f, 1f), 1);
				SunflowerWindow.WriteStatus();
				break;
			
			case Key.Number3:
			case Key.Keypad3:
				SunflowerWindow.Actors[2].Modulo += isShift ? -1 : 1;
				new FeedbackActor(SunflowerWindow.Actors[1].Modulo, new Color4(0f, 0f, 1f, 1f), 1);
				SunflowerWindow.WriteStatus();
				break;

			case Key.Number4:
			case Key.Keypad4:
			SunflowerWindow.Actors[0].Slope *= isShift ? 1/1.5 : 1.5;
				break;
			
			case Key.Number5:
			case Key.Keypad5:
			SunflowerWindow.Actors[1].Slope *= isShift ? 1/1.5 : 1.5;
				break;
			
			case Key.Number6:
			case Key.Keypad6:
			SunflowerWindow.Actors[2].Slope *= isShift ? 1/1.5 : 1.5;
				break;

			case Key.Number7:
			case Key.Keypad7:
			SunflowerWindow.Actors[0].Speed *= isShift ? 1/1.5 : 1.5;
				break;

			case Key.Number8:
			case Key.Keypad8:
			SunflowerWindow.Actors[1].Speed *= isShift ? 1/1.5 : 1.5;
				break;

			case Key.Number9:
			case Key.Keypad9:
			SunflowerWindow.Actors[2].Speed *= isShift ? 1/1.5 : 1.5;
				break;

			
			case Key.S:
				ModuloActor.SwapSortedAll();
				break;
			
			case Key.L:
				SunflowerWindow.Slope *= isShift ? 1/1.5 : 1.5;
				break;

			case Key.P:
				SunflowerWindow.Speed *= isShift ? 1/1.5 : 1.5;
				break;

			case Key.F:
				SunflowerWindow.ToggleFullscreen();
				break;

			case Key.Escape:
				SunflowerWindow.Exit();
				break;


			case Key.N:
				SunflowerWindow.ImageIndex = (SunflowerWindow.ImageIndex + (isShift ? SunflowerWindow.ImageFiles.Count - 1 : 1)) % SunflowerWindow.ImageFiles.Count;
				SunflowerWindow.Reset();
				break;

			case Key.R:
				SunflowerWindow.Offset = Complex.Zero;
				SunflowerWindow.AngleOffset = 0;
				SunflowerWindow.Reset();
				break;



			case Key.I:
				SunflowerWindow.IsInverting = !SunflowerWindow.IsInverting;
				SunflowerWindow.Reset();
				break;

			case Key.M:
				SunflowerWindow.IsMoving = !SunflowerWindow.IsMoving;
				break;
			
			case Key.Tab:
				break;
			}
		}
		
	}
}