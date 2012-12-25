using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

//using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using BenTools.Data;
using BenTools.Mathematics;
using Poincare.Geometry;

namespace Poincare.Application {
	public class FeedbackActor {
		public static List<FeedbackActor> AllActors { get; set; }

		public int Index { get; set; }
		public Color4 Color { get; set; }
		public double Duration { get; set; }
		public double Time { get; set; }

		public FeedbackActor(int index, Color4 color, double duration) {
			Index = index-1;
			Color = color;
			Duration = duration;
			Time = 0;

			if (AllActors==null) {
				AllActors = new List<FeedbackActor>();
			}

			AllActors.Add(this);
		}

		~FeedbackActor() {
			if (AllActors.Contains(this))
				AllActors.Remove(this);
		}

		public static void UpdateAll(double elapsed) {
			if (AllActors == null)
				return;

			foreach (FeedbackActor actor in AllActors.ToArray()) 
				actor.Update(elapsed);
		}

		public void Update(double elapsed) {
			Time += elapsed;

			if (Time > Duration)
				AllActors.Remove(this);
		}

		public Color4 GetColor() {
			if (Time > Duration)
				return new Color4(0f, 0f, 0f, 0f);

			return new Color4(Color.R, Color.G, Color.B, (float)(Color.A * (1 - Time / Duration)));
		}

	}
}