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

        private void SetModulo(int channel, Color4 color) {
            bool isShift = SunflowerWindow.Keyboard[Key.LShift] || SunflowerWindow.Keyboard[Key.RShift];
            SunflowerWindow.Actors[channel].Modulo += isShift ? -1 : 1;

            new FeedbackActor(SunflowerWindow.Actors[channel].Modulo, color, 8);
            if (ModuloActor.FibonacciNumbers.Contains(SunflowerWindow.Actors[channel].Modulo)) {
                ModuloActor.AnnounceFibonaccis(color);
                new FeedbackActor(SunflowerWindow.Actors[channel].Modulo, Color4.White, 2);
            }

            SunflowerWindow.WriteStatus();
        }

        private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e) {
            bool isShift = SunflowerWindow.Keyboard[Key.LShift] || SunflowerWindow.Keyboard[Key.RShift];
            bool isCtrl = SunflowerWindow.Keyboard[Key.LControl] || SunflowerWindow.Keyboard[Key.RControl];

            switch (e.Key) {

                case Key.Number1:
                case Key.Keypad1:
                    SetModulo(0, new Color4(1f, 0.5f, 0.5f, 1f));
                    break;

                case Key.Number2:
                case Key.Keypad2:
                    SetModulo(1, new Color4(0.5f, 1f, 0.5f, 1f));
                    break;

                case Key.Number3:
                case Key.Keypad3:
                    SetModulo(2, new Color4(0.5f, 0.5f, 1f, 1f));
                    break;

                case Key.Number4:
                case Key.Keypad4:
                    SunflowerWindow.Actors[0].Slope *= isShift ? 1 / 1.5 : 1.5;
                    break;

                case Key.Number5:
                case Key.Keypad5:
                    SunflowerWindow.Actors[1].Slope *= isShift ? 1 / 1.5 : 1.5;
                    break;

                case Key.Number6:
                case Key.Keypad6:
                    SunflowerWindow.Actors[2].Slope *= isShift ? 1 / 1.5 : 1.5;
                    break;

                case Key.Number7:
                case Key.Keypad7:
                    SunflowerWindow.Actors[0].Speed *= (isShift ? 1 / 1.5 : 1.5) * (isCtrl ? -1 : 1);
                    break;

                case Key.Number8:
                case Key.Keypad8:
                    SunflowerWindow.Actors[1].Speed *= (isShift ? 1 / 1.5 : 1.5) * (isCtrl ? -1 : 1);
                    break;

                case Key.Number9:
                case Key.Keypad9:
                    SunflowerWindow.Actors[2].Speed *= (isShift ? 1 / 1.5 : 1.5) * (isCtrl ? -1 : 1);
                    break;


                case Key.S:
                    ModuloActor.SwapSortedAll();
                    break;

                case Key.L:
                    SunflowerWindow.Slope *= isShift ? 1 / 1.5 : 1.5;
                    break;

                case Key.P:
                    SunflowerWindow.Speed *= isShift ? 1 / 1.5 : 1.5;
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