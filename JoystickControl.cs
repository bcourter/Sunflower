using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using Sunflower.Geometry;

namespace Sunflower.Application {
    public class JoystickControl {
        bool isLimit = false;
        bool isBraking = false;
        bool disablePQ = false;
        JoystickMapping mapping = System.Environment.OSVersion.Platform == PlatformID.Unix ? null : JoystickMapping.WindowsMapping;

        public JoystickDevice Joystick { get; private set; }

        public SunflowerWindow SunflowerWindow { get; private set; }

        public JoystickControl(JoystickDevice joystick, SunflowerWindow poincareWindow) {
            Joystick = joystick;
            SunflowerWindow = poincareWindow;

            Joystick.ButtonDown += Joystick_ButtonDown;
            Joystick.ButtonUp += Joystick_ButtonUp;
        }

        int[] buttonIsDown = new int[3];
        double[] buttonDownTime = new double[3];
        int[] SpeedDirection = new int[] { 1, 1, 1 };
        double buttonRepeatDelay = 0.6;
        private void Joystick_ButtonDown(object sender, JoystickButtonEventArgs e) {
            if (mapping.ButtonDecrement.Contains(e.Button)) {
                int index = mapping.ButtonDecrement.ToList().IndexOf(e.Button);
                SunflowerWindow.Actors[index].Modulo--;
                buttonIsDown[index] = -1;
                buttonDownTime[index] = SunflowerWindow.Now;
            }

            if (mapping.ButtonIncrement.Contains(e.Button)) {
                int index = mapping.ButtonIncrement.ToList().IndexOf(e.Button);
                SunflowerWindow.Actors[index].Modulo++;
                buttonIsDown[index] = 1;
                buttonDownTime[index] = SunflowerWindow.Now;
            }

            if (mapping.ButtonMode.Contains(e.Button))
                SunflowerWindow.Actors[mapping.ButtonMode.ToList().IndexOf(e.Button)].IsSorted = !SunflowerWindow.Actors[mapping.ButtonMode.ToList().IndexOf(e.Button)].IsSorted;

            //if (mapping.ButtonReset.Contains(e.Button))
            //    SunflowerWindow.Reset();

        }

        private void Joystick_ButtonUp(object sender, JoystickButtonEventArgs e) {
            if (mapping.ButtonDecrement.Contains(e.Button))
                buttonIsDown[mapping.ButtonDecrement.ToList().IndexOf(e.Button)] = 0;

            if (mapping.ButtonIncrement.Contains(e.Button))
                buttonIsDown[mapping.ButtonIncrement.ToList().IndexOf(e.Button)] = 0;
        }

        bool[] wasLastInReverse = new[] { false, false, false };
        public void Sample(double timing) {
            // obsolete but necessary
#pragma warning disable 0612
            SunflowerWindow.InputDriver.Poll();
#pragma warning restore 0612

            double scale = 100; // 0.001
            double limit = 0.15;
            //Console.WriteLine(string.Format("Readout Slope: ({0}, {1}, {2})", Joystick.Axis[mapping.AxisMap[0]], Joystick.Axis[mapping.AxisMap[2]], Joystick.Axis[mapping.AxisMap[4]]));
            // Console.WriteLine(string.Format("Readout Speed: ({0}, {1}, {2})", Joystick.Axis[mapping.AxisMap[1]], Joystick.Axis[mapping.AxisMap[3]], Joystick.Axis[mapping.AxisMap[5]]));

            for (int i = 0; i < 3; i++) {
                SunflowerWindow.Actors[i].Slope = Math.Pow(-Interpolation.InterpolateRange(mapping.AxisMin[i * 2], mapping.AxisMax[i * 2], (double)Joystick.Axis[mapping.AxisMap[i * 2]]) + 1.4, 2) * 12;
                double sign = Math.Sign(mapping.AxisReverse[i * 2]);
                SunflowerWindow.Actors[i].Brightness = sign * Joystick.Axis[mapping.AxisMap[i * 2]] > sign * mapping.AxisReverse[i * 2] ? 0 : 1;


                sign = Math.Sign(mapping.AxisReverse[i * 2 + 1]);
                bool isInReverse = sign * Joystick.Axis[mapping.AxisMap[i * 2 + 1]] > sign * mapping.AxisReverse[i * 2 + 1];
                if (!wasLastInReverse[i] && isInReverse)
                    SpeedDirection[i] *= -1;

                wasLastInReverse[i] = isInReverse;

                //if (i == 2)
                //    Console.WriteLine(string.Format("Readout Slope: ({0}, )", sign * Joystick.Axis[mapping.AxisMap[i * 2 + 1]] > sign * mapping.AxisReverse[i * 2 + 1]));


                SunflowerWindow.Actors[i].Speed = Math.Max(0, Interpolation.InterpolateRange(mapping.AxisMin[i * 2 + 1], mapping.AxisMax[i * 2 + 1], (double)Joystick.Axis[mapping.AxisMap[i * 2 + 1]])) * 4 * SpeedDirection[i];

                if (buttonIsDown[i] != 0 && SunflowerWindow.Now - buttonDownTime[i] > buttonRepeatDelay)
                    SunflowerWindow.Actors[i].Modulo += buttonIsDown[i];


            }



            //   Console.WriteLine(string.Format("Slope: ({0}, {1}, {2})", SunflowerWindow.Actors[0].Slope, SunflowerWindow.Actors[1].Slope, SunflowerWindow.Actors[2].Slope));
            //  Console.WriteLine(string.Format("Speed: ({0}, {1}, {2})", SunflowerWindow.Actors[0].Speed, SunflowerWindow.Actors[1].Speed, SunflowerWindow.Actors[2].Speed));
        }
    }

    public class JoystickMapping {
        public JoystickButton[] ButtonIncrement { get; set; }
        public JoystickButton[] ButtonDecrement { get; set; }
        public JoystickButton[] ButtonMode { get; set; }
        public JoystickButton[] ButtonReset { get; set; }


        public int[] AxisMap { get; set; }

        public readonly double[] AxisMin = new[] { 0.40, -0.55, -0.57, 0.53, 0.50, -0.48 };
        public readonly double[] AxisMax = new[] { -0.98, 0.89, 0.87, -0.92, -0.92, 0.96 };
        public readonly double[] AxisDetent = new[] { 0.73, -.79, -0.82, 0.74, 0.76, -0.73 };
        public readonly double[] AxisReverse = new[] { 0.84, -0.84, -0.98, 0.84, 0.84, -0.84 };

        private JoystickMapping() {
        }

        public static JoystickMapping WindowsMapping {
            get {
                JoystickMapping mapping = new JoystickMapping();
                mapping.ButtonDecrement = new[] { JoystickButton.Button0, JoystickButton.Button4, JoystickButton.Button8 };
                mapping.ButtonIncrement = new[] { JoystickButton.Button1, JoystickButton.Button5, JoystickButton.Button9 };
                mapping.ButtonReset = new[] { JoystickButton.Button2, JoystickButton.Button6, JoystickButton.Button10 };
                mapping.ButtonMode = new[] { JoystickButton.Button3, JoystickButton.Button7, JoystickButton.Button11 };


                mapping.AxisMap = new[] { 0, 1, 2, 3, 4, 5 };

                return mapping;
            }
        }

    }
}
