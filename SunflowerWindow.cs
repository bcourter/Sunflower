using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

using BenTools.Data;
using BenTools.Mathematics;
using Poincare.Geometry;

namespace Poincare.Application {
	public class SunflowerWindow : GameWindow {
		static GraphicsMode graphicsMode;
		const int windowDefaultSize = 800;
		double actualTime = System.DateTime.Now.Ticks / TimeSpan.TicksPerMinute;
		double lastActualTime;
		double time = 0;
		double resetTime = 0;
		double resetDuration = 60;
//		JoystickControl joystickControl = null;
		MouseControl mouseControl = null;
		int p = 5, q = 5 ;
		int imageIndex = 0;
		List<double> stern = new List<double>();

		public bool IsSorted { get; set; }

		public double Speed { get; set; }
		public double Slope{ get; set; }
		
		//delete or move these...
		public Complex Offset { get; set; }

		public double AngleOffset { get; set; }

		public bool IsMoving { get; set; }

		public bool IsRandomizing { get; set; }
		
		public static List<string> ImageFiles{ get; set; }
		
		public double ImageSpeed { get; set; }

		public double ImageOffset { get; set; }

		public bool IsInverting { get; set; }
		//...

		/// <summary>Creates a window with the specified title.</summary>
		public SunflowerWindow()
			: base(windowDefaultSize, windowDefaultSize, graphicsMode, "Poincare'") {
			VSync = VSyncMode.Off;
	
			IsSorted = true;

			Speed = 4;
			Slope = 4;

			Offset = Complex.Zero;
			AngleOffset = 0;
			
			IsMoving = false;
			IsRandomizing = false;
			
			ImageSpeed = 0.04;
			ImageOffset = 0;
			IsInverting = false;

			stern.Add(1);
			List<double> lastRow = new List<double>();
			lastRow.Add(1);
			for (int i = 1; i < 12; i++) {
				int size = (int) Math.Pow(2, i-1);
				List<double> firstHalf = new List<double>(size);
				List<double> secondHalf = new List<double>(size);
				for (int j = 0; j < size; j++) {
					firstHalf.Add(lastRow[j]);
					secondHalf.Add(lastRow[j] + lastRow[size - j - 1]);
				}

				lastRow = firstHalf;
				lastRow.AddRange(secondHalf);
				stern.AddRange(lastRow);
			}

		}

		/// <summary>Load resources here.</summary>
		/// <param name="e">Not used.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);		
			
			new KeyboardControl(this);
			mouseControl = new MouseControl(this);
			
//			if (Joysticks.Count == 1)
//				joystickControl = new JoystickControl(Joysticks[0], this);
			
			Reset();
		}

		public void ToggleFullscreen() {
			if (WindowState == WindowState.Fullscreen) {
				WindowState = WindowState.Normal;
				Width = windowDefaultSize;
				Height = windowDefaultSize;
				OnResize(null);
				return;
			}
			
			WindowState = WindowState.Fullscreen;
		}

		protected override void OnUnload(EventArgs e) {
		}

		/// <summary>
		/// Called when your window is resized. Set your viewport here. It is also
		/// a good place to set up your projection matrix (which probably changes
		/// along when the aspect ratio of your window).
		/// </summary>
		/// <param name="e">Not used.</param>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

			//jetblbc	Height = Width;
			float aspect = (float)Height / Width;
			Matrix4 projection = Matrix4.CreateOrthographic(2 / aspect, 2, -1f, 1f);
			projection *= Matrix4.CreateRotationY((float)Math.PI);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref projection);
		}

		/// <summary>
		/// Called when it is time to setup the next frame. Add you game logic here.
		/// </summary>
		/// <param name="e">Contains timing information for framerate independent logic.</param>
		protected override void OnUpdateFrame(FrameEventArgs e) {
			base.OnUpdateFrame(e);
		}

		List<List<Complex>> polygons = new List<List<Complex>>();
		double Phi = (Math.Sqrt(5) + 1) / 2;
		double Tau = Math.PI * 2;
		double scale = 3e-2;
		ModuloActor[] actors = new ModuloActor[3];
		SortedDictionary<Vector, List<VoronoiEdge>> cells;
		public void Reset() {
			GL.Enable(EnableCap.LineSmooth);
			GL.Enable(EnableCap.PolygonSmooth);
			//		GL.Enable(EnableCap.DepthTest);
			Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);
			
			int seeds = 1024;
			int extraSeeds = 256;
			Vector[] points = new Vector[seeds + extraSeeds];

			double thetaOffset = -Tau / Phi + Tau/4;
			for (int i = 0; i < seeds + extraSeeds; i++) {
			    double theta = (double) (i+1) * Tau / Phi;
                double r = Math.Sqrt( i);
			    double x = (r) * Math.Cos(theta + thetaOffset);
			    double y = (r) * Math.Sin(theta + thetaOffset);
			    
				points[i] = new Vector(new double[] {x, y});
			}

			VoronoiGraph graph = Fortune.ComputeVoronoiGraph(points);

            cells = new SortedDictionary<Vector, List<VoronoiEdge>>();
            foreach (VoronoiEdge edge in graph.Edges) {
                if (double.IsNaN(edge.VVertexA.X) ||
                    double.IsNaN(edge.VVertexA.Y) ||
                    double.IsNaN(edge.VVertexB.X) ||
                    double.IsNaN(edge.VVertexB.Y)
				)
					continue;

                if (!cells.ContainsKey(edge.LeftData))
                    cells[edge.LeftData] = new List<VoronoiEdge>();

                cells[edge.LeftData].Add(edge);

                if (!cells.ContainsKey(edge.RightData))
                    cells[edge.RightData] = new List<VoronoiEdge>();

                cells[edge.RightData].Add(edge);
                
                Complex pA = new Complex(edge.VVertexA.X , edge.VVertexA.Y);
                Complex pB = new Complex(edge.VVertexB.X , edge.VVertexB.Y);

				int sampleCount = 2;
				Complex[] samples = new Complex[sampleCount];
				samples[0] = pA;
				samples[sampleCount - 1] = pB;
				for (int i = 1; i < sampleCount - 1; i++) {
					double ratio = (double) i / sampleCount;
					samples[i] = pA * (1-ratio) + pB * ratio;
				}

            }
				
			for (int i = 0; i < seeds; i++) {
				Queue<VoronoiEdge> edges =new Queue<VoronoiEdge>(cells.Values.ElementAt(i));
				var firstEdge = edges.Dequeue();
				List<Complex> polygonPoints = new List<Complex>();
				polygonPoints.Add(new Complex(firstEdge.VVertexA.X * scale, firstEdge.VVertexA.Y * scale));
				polygonPoints.Add(new Complex(firstEdge.VVertexB.X * scale, firstEdge.VVertexB.Y * scale));
				while (edges.Count > 0) {
					var edge = edges.Dequeue();
					Complex pA = new Complex(edge.VVertexA.X * scale, edge.VVertexA.Y * scale);
					Complex pB = new Complex(edge.VVertexB.X * scale, edge.VVertexB.Y * scale);

					if (polygonPoints[0] == pA) {
						polygonPoints.Insert(0, pB);
						continue;
					}
					if (polygonPoints[0] == pB) {
						polygonPoints.Insert(0, pA);
						continue;
					}

					if (polygonPoints[polygonPoints.Count -1] == pA) {
						polygonPoints.Add(pB);
						continue;
					}
					if (polygonPoints[polygonPoints.Count -1] == pB) {
						polygonPoints.Add(pA);
						continue;
					}

					edges.Enqueue(edge);
				}

				polygons.Add(polygonPoints);
			}

			for (int i = 0; i <= ModuloActor.MaxMod; i++)
				ModuloActor.Maps[i] = CreateIndexMap(i, cells);

			actors[0] = new ModuloActor(0, 0);
			actors[1] = new ModuloActor(0, 1 / 3);
			actors[2] = new ModuloActor(0, 2 / 3);

			ModuloActor.AnnounceFibonaccis();
		}

		private int[] CreateIndexMap(int size, IDictionary<Vector, List<VoronoiEdge>> cells) {
			var centers = cells.Keys
				.Take(size * 3)
				.Skip(size * 2)
				.Select(v => new Complex(v.X * scale, v.Y * scale))
				.ToArray();

			var ordered = centers
				.OrderBy(c => c.Argument)
				.ToList();

			var value = new int[size];
			for (int i = 0; i < size; i++) 
				value[i] = ordered.IndexOf(centers[i]);

			return value;
		}


		/// <summary>
		/// Called when it is time to render the next frame. Add your rendering code here.
		/// </summary>
		/// <param name="e">Contains timing information.</param>
		protected override void OnRenderFrame(FrameEventArgs e) {
			//GC.Collect();

			//	GC.WaitForPendingFinalizers();
			base.OnRenderFrame(e);
			
			lastActualTime = actualTime;
			actualTime = (double)System.DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
			double elapsed = (actualTime - lastActualTime) * Speed;
			time += elapsed;

			//			Console.WriteLine(string.Format("Frame:{0:F5} Avg:{1:F5}", time - oldTime, (time - startTime) / ++drawCount));
			
			if (IsRandomizing && time - resetTime > resetDuration) {
	//			Randomize();
				resetTime = time;
			}
			
//			if (joystickControl != null)
//				joystickControl.Sample(disc.DrawTime);
			
			mouseControl.Sample();

			GL.ClearColor(Color4.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			ModuloActor.UpdateAll(elapsed);
			FeedbackActor.UpdateAll(elapsed);

			var FeedbackActorMap = new Dictionary<int, FeedbackActor[]>();
			if (FeedbackActor.AllActors != null)
				FeedbackActorMap = FeedbackActor.AllActors
					.GroupBy(a => a.Index)
					.ToDictionary(g => g.Key, g => g.ToArray())
				;
						
			for (int i = 0; i < polygons.Count; i++) {	
				var color = new Color4(
                    (float) actors[0].GetValue(i),
                    (float) actors[1].GetValue(i),
                    (float) actors[2].GetValue(i),
                    1f);

				if (FeedbackActorMap.ContainsKey(i)) {
					foreach (FeedbackActor actor in FeedbackActorMap[i])
						color = BlendColors(actor.GetColor(), color);
				}
                   
				var polygonPoints = polygons[i];
				GL.Begin(BeginMode.TriangleFan);  
				GL.Color4(color);
				for (int j = 0; j < polygonPoints.Count; j++)
					GL.Vertex3(polygonPoints[j].Vector3d);            
				GL.End();  

				for (int j = 1; j < polygonPoints.Count; j++) 
					new TrimmedCircLine(polygonPoints[j-1], polygonPoints[j]).DrawGL(new Color4(0.1f, 0.1f, 0.1f, 1));

			}


//			var random = new Random();
//			new Complex(random.NextDouble()*scale, random.NextDouble()*scale).DrawGL(Color4.Wheat);

			//PolarDemo(time);
			//	LoopDemo(mousePos.Re);

		//	SaveGL(time.ToString("000000000.000000"));

			SwapBuffers();
		}

		// wikipedia alpha_compostiting
		private	Color4 BlendColors(Color4 source, Color4 dest) {
			float outA = source.A + dest.A * (1 - source.A);

			return new Color4(
				(source.R * source.A + dest.R * dest.A * (1-source.A)) / outA,
				(source.G * source.A + dest.G * dest.A * (1-source.A)) / outA,
				(source.B * source.A + dest.B * dest.A * (1-source.A)) / outA,
				outA
			);
		}

		// http://www.opengl.org/discussion_boards/showthread.php/165932-Capture-OpenGL-screen-C/page2
		public Bitmap SaveGL() {
			Bitmap bmp = new Bitmap(Width, Height);
			System.Drawing.Imaging.BitmapData data = bmp.LockBits(this.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, 
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			GL.Finish();
			bmp.UnlockBits(data);
			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
			return bmp;
		}

        private void SaveGL(string timestamp) {
            Bitmap bmp = SaveGL();
           bmp.Save("/home/blake/Projects/Poincare/Poincare/output/" + timestamp + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        //    bmp.Save("/dev/null", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

		void LoopDemo(double t) {
			int pointCount = 100;
			Complex[] gamma = new Complex[100];
			Complex[] gammaF = new Complex[100];

			List<Complex > zeros = new List<Complex>();
			zeros.Add(new Complex(0.1, 0.1));
			zeros.Add(new Complex(-0.3, 0.3));
			zeros.Add(new Complex(-0.5, -0.5));

			for (int i = 0; i < pointCount; i++) {
				gamma[i] = Complex.CreatePolar(t, (double)i / pointCount * Math.PI * 2);
				gammaF[i] = Complex.One;
				
				foreach (Complex zero in zeros)
					gammaF[i] *= gamma[i] - zero;
			}

			Circle.Create(Complex.Zero, 0.02).DrawGL(Color4.Gray);
			foreach (Complex zero in zeros)
				Circle.Create(zero, 0.02).DrawGL(Color4.White);

			GL.Begin(BeginMode.LineLoop);
			GL.Color4(Color.Blue);
			for (int i = 0; i < pointCount; i++)
				GL.Vertex3(gamma[i].Vector3d);
			GL.End();

			GL.Begin(BeginMode.LineLoop);
			GL.Color4(Color.Red);
			for (int i = 0; i < pointCount; i++)
				GL.Vertex3(gammaF[i].Vector3d);
			GL.End();

			Complex aa = Complex.One * 3;
			Complex bb = -(2 * zeros[0] + 2 * zeros[1] + 2 * zeros[2]);
			Complex cc = zeros[0] * zeros[1] + zeros[1] * zeros[2] + zeros[2] * zeros[0];

			Complex r1 = (-bb + (bb * bb - 4 * aa * cc).Sqrt) / 2 / aa;
			Complex r2 = (-bb - (bb * bb - 4 * aa * cc).Sqrt) / 2 / aa;
			Circle.Create(r1, 0.01).DrawGL(Color4.Green);

			Circle.Create(r2, 0.01).DrawGL(Color4.Green);
		}

		void PolarDemo(double time) {
			//	CircLine inversionCircle = Circle.Create(new Complex(-1, -0.5), 1);
			CircLine inversionCircle = Circle.Create(new Complex(0, 0), 1);
			Mobius inversion = inversionCircle.AsInversion;
			inversionCircle.DrawGL(Color4.Gray);

			int circleCount = 6;
			Complex center = Complex.CreatePolar(0.3, time);
			for (int i = 0; i < circleCount; i++) {
				CircLine radialLine = Line.Create(center, Math.PI * i / circleCount);
				radialLine.DrawGL(Color4.Green);
				CircLine radialLineImage = inversion * radialLine.Conjugate;
				radialLineImage.DrawGL(Color4.GreenYellow);
				CircLine radialLineBack = inversion * radialLineImage.Conjugate;
				radialLineBack.DrawGL(Color4.Aqua);

				//CircLine circumferencialLine = Circle.Create(center, (double) (i + 1) / circleCount);
				//circumferencialLine.DrawGL(Color4.DarkRed);
				//CircLine circumferencialLineImage = inversion * circumferencialLine.Conjugate;
				//circumferencialLineImage.DrawGL(Color4.Red);

			}
		}

		public void WriteStatus() {
			Console.WriteLine(string.Format("({0}, {1}, {2})", actors[0].Modulo,  actors[1].Modulo,  actors[2].Modulo));
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			//		graphicsMode = new GraphicsMode(GraphicsMode.Default.ColorFormat, GraphicsMode.Default.Depth, GraphicsMode.Default.Stencil, graphicsModeSamples);
			graphicsMode = new GraphicsMode();
			
			// The 'using' idiom guarantees proper resource cleanup.
			// We request 30 UpdateFrame events per second, and unlimited
			// RenderFrame events (as fast as the computer can handle).
			using (SunflowerWindow game = new SunflowerWindow()) {
				game.Run(30.0);
			}
		}


		public int P { 
			get { return p;	}
			set {
				p = value;
				while ((p - 2) * (q - 2) <= 4)
					p++;
			}
		}
			
		public int Q { 
			get { return q;	}
			set {
				q = value;
				while ((p - 2) * (q - 2) <= 4)
					q++;
				
				//	q = Math.Max(q, 4);
			}
		}

		public int ImageIndex { 
			get { return imageIndex;	}
			set {
				imageIndex = value;
				while (imageIndex < 0)
					imageIndex += ImageFiles.Count;
				
				imageIndex %= ImageFiles.Count;
			}
		}

		public double Time {
			get { return time;}
		}

		public ModuloActor[] Actors {
			get { return actors;}
		}

		public IDictionary<Vector, List<VoronoiEdge>> Cells {
			get { return cells; }
		}
}
}