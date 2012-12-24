using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using BenTools.Data;
using BenTools.Mathematics;
using Poincare.Geometry;

namespace Poincare.Application {
	public class ModuloActor {
		public const int MaxMod = 233;
		static List<ModuloActor> AllActors { get; set; }
		static int[][] maps = new int[MaxMod+1][];

		int modulo;

		public double Speed { get; set; }
		public double Slope { get; set; }
		public int Offset { get; set; }
		public bool IsInverted { get; set; }
		public bool IsSorted { get; set; }
		public double Time { get; set; }
		public double FadeInTime { get; set; }

		public static readonly List<int> FibonacciNumbers = new List<int>();
		
		public ModuloActor(int modulo, int offset) {
			Modulo = modulo;
			Offset = offset;
			Speed = 1;
			Slope = 4;
			IsInverted = false;
			IsSorted = true;
			Time = 0;
			FadeInTime = 1.6;

			if (AllActors==null) {
				AllActors = new List<ModuloActor>();

				FibonacciNumbers.Add(1);
				FibonacciNumbers.Add(2);
				for (int i = 2; FibonacciNumbers.Last() < 1024; i++) 
					FibonacciNumbers.Add(FibonacciNumbers[i-1] + FibonacciNumbers[i-2]);
			}

			AllActors.Add(this);
		}

		~ModuloActor() {
			AllActors.Remove(this);
		}

		public static void UpdateAll(double elapsed) {
			foreach (ModuloActor actor in AllActors)
				actor.Update(elapsed);
		}

		public static void SwapSortedAll() {
			foreach (ModuloActor actor in AllActors)
				actor.IsSorted = !actor.IsSorted;
		}

		public static void InvertAll() {
			foreach (ModuloActor actor in AllActors)
				actor.IsInverted = !actor.IsInverted;
		}

		public static void AnnounceFibonaccis() {
			AnnounceFibonaccis(new Color4(1f, 1f, 1f, 0.5f));
		}

		public static void AnnounceFibonaccis(Color4 color) {
			foreach (int n in ModuloActor.FibonacciNumbers)
				new FeedbackActor(n, color, 5);
		}

		public void Update(double elapsed) {
			Time += elapsed * Speed;
		}

		public double GetValue(int index) {
			if (modulo == 0)
				return 0;

			index = IsSorted ? 
				maps[modulo][(index + Offset * modulo) % modulo] :
				index % modulo;

			double center = Time % modulo;
			double distance = Math.Min(Math.Min(Math.Abs(index - modulo - center), Math.Abs(index - center)), Math.Abs(index + modulo - center));
			return Math.Sqrt(1 - Slope/modulo * distance) * (Time < FadeInTime ? Math.Max(Time/FadeInTime, 0.25) : 1);
		}

		public static int[][] Maps { 
			get { return maps; } 
			set { maps = value; }
		}
		
		public int Modulo { 
			get { return modulo; } 
			set { 
				modulo = Math.Max(Math.Min(value, MaxMod), 0);
				Time = 0;
			}
		}
		
	}
}