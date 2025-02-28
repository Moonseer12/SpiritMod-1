using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Utilities;
using SpiritMod.NPCs.StarjinxEvent.Enemies.Pathfinder;

namespace SpiritMod.NPCs.StarjinxEvent.Enemies.Starachnid
{
	public class SubStar //Substars are stars that draw along threads and quickly fade
	{
		public int Counter; //Counter to keep track of time
		public float MaxScale; //Maximum size they can reach

		public Vector2 Position; //Position of the substar

		public SubStar(float maxScale, Vector2 position)
		{
			Counter = 0;
			MaxScale = maxScale;
			Position = position;
		}

		public void Update() => Counter++;
	}

	public class StarThread
	{
		public Vector2 StartPoint;
		public Vector2 EndPoint;

		public int Counter;
		public int Duration = 800; //How long the thread lasts
		public int Length; //Length of the thread

		public float StartScale = 1; //Scale of start point star
		public float EndScale = 1; //Scale of end point star

		public bool DrawStart = true;

		public List<SubStar> SubStars = new List<SubStar>(); //list of stars that twinkle along the string

		public StarThread(Vector2 startPoint, Vector2 endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
			Counter = 0;
			Length = (int)(startPoint - endPoint).Length();
		}

		public void Update()
		{
			Counter++;
			foreach (SubStar star in SubStars.ToArray()) //update substars
			{
				star.Update();
				if (star.Counter > 31)
					SubStars.Remove(star);
			}
			if (Main.rand.Next(Math.Max((int)((1f / Length) * 20000), 1)) + 1 < 4)
				SubStars.Add(new SubStar(Main.rand.NextFloat(0.3f, 0.5f), Vector2.Lerp(StartPoint, EndPoint, Main.rand.NextFloat()) + (Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.Next(15, 40)))); //super magic number-y line, not super proud of this
		}
	}

	public class Starachnid : ModNPC, IStarjinxEnemy, IDrawAdditive
	{
		public List<StarThread> threads = new List<StarThread>(); //All active threads the spider has weaved
		public StarThread currentThread; //The current thread the starachnid is on
		public float progress; //Progress along current thread

		private bool initialized = false; //Has the thread been started?
		private float toRotation = 0; //The rotation for the spider to rotate to
		private float threadRotation = 0; //Current rotation of the thread
		private float speed = 2; //The walking speed of the spider
		private int threadCounter = 0; //How long has it been since last star?

		internal const float THREADGROWLERP = 30; //How many frames does it take for threads to fade in/out?
		internal const int DISTANCEFROMPLAYER = 300; //How far does the spider have to be from the player to turn around?

		private int seed; // The seed shared between the server and clients for the starachshit
		private bool seedInitialized;
		private UnifiedRandom random; // The random instance created with the above seed, if in multiplayer

		private Vector2 Bottom
		{
			get => NPC.Center + ((NPC.height / 2) * (toRotation + 1.57f).ToRotationVector2());
			set => NPC.Center = value - ((NPC.height / 2) * (toRotation + 1.57f).ToRotationVector2());
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starachnid");
			Main.npcFrameCount[NPC.type] = 8;
		}

		public override bool IsLoadingEnabled(Mod mod) => false;

		public override void SetDefaults()
		{
			NPC.width = 64;
			NPC.height = 64;
			NPC.damage = 70;
			NPC.defense = 28;
			NPC.lifeMax = 450;
			NPC.HitSound = SoundID.NPCHit3;
			NPC.DeathSound = SoundID.DD2_LightningBugDeath;
			NPC.value = 600f;
			NPC.knockBackResist = 0;
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			int seed = Main.rand.Next();
			random = new UnifiedRandom(seed);

			if (Main.netMode == NetmodeID.Server)
				NPC.netUpdate = true;
		}

		public override void SendExtraAI(BinaryWriter writer) => writer.Write(seed);

		// If in multiplayer we receive the seed from the server and create the starachnid's random instance with it
		// The starachnid does not update in multiplayer until the seed is received
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int seed = reader.ReadInt32();

			if (!seedInitialized)
			{
				this.seed = seed;
				random = new UnifiedRandom(seed);
				seedInitialized = true;

				initialized = false;
				toRotation = 0f;
				threadRotation = 0f;
				threadCounter = 0;
				threads.Clear();
				currentThread = null;
				progress = 0f;
			}
		}

		public override void AI()
		{
			if (!seedInitialized && Main.netMode != NetmodeID.SinglePlayer)
				return;

			NPC.TargetClosest(false);

			if (!initialized)
			{
				SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
				initialized = true;
				NewThread(true, true);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<StarachnidProj>(), Main.expertMode ? 20 : 45, 0, NPC.target, NPC.whoAmI);
			}

			TraverseThread(); //Walk along thread
			RotateIntoPlace(); //Smoothly rotate into place
			UpdateThreads(); //Update the spider's threads

			if (progress >= 1) //If it's at the end of it's thread, create a new thread
				NewThread(false, true);
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int i = 0; i < 12; i++)
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.VilePowder, 2.5f * hitDirection, -2.5f, 0, default, Main.rand.NextFloat(.45f, .75f));

			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
                for (int k = 0; k < 4; k++)
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Starachnid1").Type, Main.rand.NextFloat(.6f, 1f));
				ThreadDeathDust();
			}
		}

		public override Color? GetAlpha(Color drawColor) => Color.Lerp(drawColor, Color.White, 0.5f) * NPC.Opacity;
		public void DrawPathfinderOutline(SpriteBatch spriteBatch) => PathfinderOutlineDraw.DrawAfterImage(spriteBatch, NPC, NPC.frame, Vector2.Zero, NPC.frame.Size() / 2);

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - Main.screenPosition, NPC.frame, GetAlpha(drawColor).Value, NPC.rotation % 6.28f, NPC.frame.Size() / 2, NPC.scale, SpriteEffects.FlipHorizontally, 0);

			return false;
		}

		public void AdditiveCall(SpriteBatch sB, Vector2 screenPos)
		{
			DrawThreads(sB);
			sB.Draw(ModContent.Request<Texture2D>(Texture + "_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation % 6.28f, NPC.frame.Size() / 2, NPC.scale, SpriteEffects.FlipHorizontally, 0);
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter %= Main.npcFrameCount[NPC.type];
			int frame = (int)NPC.frameCounter;
			NPC.frame.Y = frame * frameHeight;
			NPC.frameCounter += 0.30f;
		}

		private void NewThread(bool firstThread, bool mainThread)
		{
			if (!firstThread && mainThread)
				threads.Add(currentThread);

			Vector2 startPos = Bottom;
			int maxDistance = random.Next(200, 500);
			if (!mainThread)
				maxDistance = (int)(maxDistance * 0.5f);

			int distance = 0;
			Vector2 direction = NewThreadAngle(maxDistance, mainThread, ref distance, startPos); //Get both the direction (direction) and the length (i) of the next thread

			var thread = new StarThread(startPos, startPos + (direction * distance));

			Vector2 newPos = Vector2.Zero; //make star overlap if theres a nearby star

			if (!firstThread)
				if (NearbyStars(startPos + (direction * distance), ref newPos))
					thread = new StarThread(startPos, newPos);

			thread.StartScale = random.NextFloat(0.5f, 0.8f);
			thread.EndScale = random.NextFloat(0.5f, 0.8f);

			if (mainThread)
			{
				thread.DrawStart = false;
				currentThread = thread;
				progress = 0;
				threadRotation = direction.ToRotation();
			}
			else
			{
				threads.Add(thread);
				thread.Duration -= (currentThread.Counter + (int)THREADGROWLERP); //Make sure it fades away before the thread it's attached to!
				threadCounter = 45; //45 ticks until stars are able to spawn again
			}
		}

		//The follow method runs 2 while loops to try and avoid tiles
		//Both while loops cast out from the bottom of the spider, in a random angle, and try to reach their set distance without hitting a tile. If they can't, they try again
		//Both have "tries" to make sure it doesn't try over 100 times
		//The first while loop, the new angle is in the same GENERAL direction as the current thread
		//The second one can go in any direction
		//However, if the player is too far away, it curves towards them
		private Vector2 NewThreadAngle(int maxDistance, bool mainThread, ref int dist, Vector2 startPos)
		{
			Player player = Main.player[NPC.target];
			Vector2 distanceFromPlayer = player.Center - startPos;
			Vector2 direction = Vector2.One;
			int tries = 0;

			if (distanceFromPlayer.Length() > DISTANCEFROMPLAYER && mainThread)
			{
				while (dist < maxDistance) //Loop through the shortest angle to the player, but multiplied by a random float (above 0.5f)
				{
					float rotDifference = ((((distanceFromPlayer.ToRotation() - threadRotation) % MathHelper.TwoPi) + 9.42f) % MathHelper.TwoPi) - MathHelper.Pi;
					direction = (threadRotation + (Math.Sign(rotDifference) * random.NextFloat(1f, 1.5f))).ToRotationVector2();

					for (dist = 16; dist < maxDistance; dist++)
					{
						Vector2 toLookAt = startPos + (direction * dist);
						if (IsTileActive(toLookAt))
							break;
					}

					if (tries++ > 100)
						break;
				}

				Vector2 finalPos = startPos + (direction * dist); //Top of the world check
				if (finalPos.Y < 590 && direction.Y < 0)
					direction.Y *= -1;

				if (ModContent.GetInstance<StarjinxEventWorld>().StarjinxActive && Vector2.Distance(finalPos, player.GetModPlayer<StarjinxPlayer>().StarjinxPosition) > StarjinxMeteorite.EVENT_RADIUS - 50)
				{ //Check for leaving event area
					direction = Vector2.Normalize(player.GetModPlayer<StarjinxPlayer>().StarjinxPosition - startPos);
					dist = maxDistance;
				}

				tries = 0;
				if (dist < maxDistance) //Runs if the current angle would make too short of a thread
				{
					while (dist < maxDistance)
					{
						direction = random.NextFloat(MathHelper.TwoPi).ToRotationVector2();
						for (dist = 16; dist < maxDistance; dist++)
						{
							Vector2 toLookAt = startPos + (direction * dist);
							if (IsTileActive(toLookAt))
								break;
						}
						tries++;
						if (tries > 100)
							break;
					}
				}
			}
			else
			{
				while (dist < maxDistance) //first while loop - get angle
				{
					if (mainThread)
						direction = (threadRotation + random.NextFloat(-1f, 1f)).ToRotationVector2(); //Woohoo magic numbers
					else
						direction = random.NextFloat(MathHelper.TwoPi).ToRotationVector2();

					for (dist = 16; dist < maxDistance; dist++)
					{
						Vector2 toLookAt = startPos + (direction * dist);
						if (IsTileActive(toLookAt))
							break;
					}

					if (tries++ > 100)
						break;
				}

				Vector2 finalPos = startPos + (direction * dist); //Top of the world check
				if (finalPos.Y < 590 && direction.Y < 0)
					direction.Y *= -1;

				if (ModContent.GetInstance<StarjinxEventWorld>().StarjinxActive && Vector2.Distance(finalPos, player.GetModPlayer<StarjinxPlayer>().StarjinxPosition) > StarjinxMeteorite.EVENT_RADIUS)
				{ //Check for leaving event area
					direction = Vector2.Normalize(player.GetModPlayer<StarjinxPlayer>().StarjinxPosition - startPos);
					dist = 2;
				}

				tries = 0;
				if (dist < maxDistance) //Runs if the current angle would make too short of a thread
				{
					while (dist < maxDistance)
					{
						direction = random.NextFloat(MathHelper.TwoPi).ToRotationVector2();
						for (dist = 16; dist < maxDistance; dist++)
						{
							Vector2 toLookAt = startPos + (direction * dist);
							if (IsTileActive(toLookAt))
								break;
						}

						if (tries++ > 100)
							break;
					}
				}
			}
			return direction;
		}

		private bool NearbyStars(Vector2 position, ref Vector2 output)
		{
			int maxDistance = 40;
			float distance;

			foreach (StarThread thread in threads.ToArray())
			{
				distance = (thread.StartPoint - position).Length();
				if (distance < maxDistance)
				{
					output = thread.StartPoint;
					return true;
				}
				distance = (thread.EndPoint - position).Length();
				if (distance < maxDistance)
				{
					output = thread.EndPoint;
					return true;
				}
			}

			distance = (currentThread.StartPoint - position).Length();

			if (distance < maxDistance)
			{
				output = currentThread.StartPoint;
				return true;
			}

			distance = (currentThread.EndPoint - position).Length();

			if (distance < maxDistance)
			{
				output = currentThread.EndPoint;
				return true;
			}
			return false;
		}

		private bool IsTileActive(Vector2 toLookAt) //Is the tile at the vector position solid?
		{
			Point tPos = toLookAt.ToTileCoordinates();
			if (WorldGen.InWorld(tPos.X, tPos.Y, 2) && Framing.GetTileSafely(tPos.X, tPos.Y).HasTile)
				return Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16f), (int)(toLookAt.Y / 16f)).TileType];

			return false;
		}

		private void TraverseThread()
		{
			progress += (1f / currentThread.Length) * speed;
			Bottom = Vector2.Lerp(currentThread.StartPoint, currentThread.EndPoint, progress);
			threadCounter--;
			//if (random.Next(200) == 0 && threadCounter < 0 && progress > 0.15f && progress < 0.85f)
			//	NewThread(false, false);
			if (!Main.dedServ && progress < 0.9f && progress > 0) //Make particles along the thread while starachnid walks
			{
				for(int i = -1; i <= 1; i++)
				{
					Vector2 vel = Vector2.Normalize(NPC.position - NPC.oldPosition);
					Particles.ParticleHandler.SpawnParticle(new Particles.GlowParticle(Bottom, vel.RotatedBy(Main.rand.NextFloat(MathHelper.Pi / 16, MathHelper.Pi / 6) * i),
						Color.HotPink, Main.rand.NextFloat(0.05f, 0.1f), 22, 15));
				}
			}
		}

		private void RotateIntoPlace()
		{
			toRotation = (currentThread.EndPoint - currentThread.StartPoint).ToRotation();

			bool empowered = NPC.GetGlobalNPC<PathfinderGNPC>().Buffed;
			float speedInc = empowered ? 1.4f : 1f;

			float rotDifference = ((((toRotation - NPC.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
			if (Math.Abs(rotDifference) < 0.15f)
			{
				NPC.rotation = toRotation;
				speed = 2 * speedInc;
				return;
			}
			speed = 1 * speedInc;
			NPC.rotation += Math.Sign(rotDifference) * 0.06f;
		}

		private void DrawThreads(SpriteBatch spriteBatch)
		{
			if (currentThread == null)
				return;

			float length;
			Texture2D tex = ModContent.Request<Texture2D>("SpiritMod/Textures/Trails/Trail_4", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Vector2 threadScale = new Vector2(1 / (float)tex.Width, 30 / (float)tex.Height); //Base scale of the thread based on the texture's size, stretched horizontally depending on thread length

			//Draw each thread's beam
			foreach (StarThread thread in threads) 
			{
				length = Math.Min(thread.Counter / THREADGROWLERP, (thread.Duration - thread.Counter) / THREADGROWLERP);
				length = Math.Min(length, 1);
				spriteBatch.Draw(tex, thread.StartPoint - Main.screenPosition, null, Color.HotPink * length, (thread.EndPoint - thread.StartPoint).ToRotation(), new Vector2(0f, tex.Height / 2), 
					threadScale * new Vector2(thread.Length, 1), SpriteEffects.None, 0f);
			}
			StarThread thread2 = currentThread;

			float size = Math.Min(thread2.Counter / THREADGROWLERP, (thread2.Duration - thread2.Counter) / THREADGROWLERP);
			size = Math.Min(size, 1);

			spriteBatch.Draw(tex, thread2.StartPoint - Main.screenPosition, null, Color.HotPink, (thread2.EndPoint - thread2.StartPoint).ToRotation(), //Draw the portion of the current beam that's already been walked through
				new Vector2(0f, tex.Height / 2), threadScale * new Vector2(progress * thread2.Length, 1), SpriteEffects.None, 0f);

			spriteBatch.Draw(tex, thread2.EndPoint - Main.screenPosition, null, Color.HotPink * 0.5f * size, (thread2.StartPoint - thread2.EndPoint).ToRotation(), //Draw the remaining portion at lower opacity
				new Vector2(0f, tex.Height / 2), threadScale * new Vector2((1 - progress) * thread2.Length, 1), SpriteEffects.None, 0f);

			tex = ModContent.Request<Texture2D>("SpiritMod/NPCs/StarjinxEvent/Enemies/Starachnid/SpiderStar", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			Texture2D Bloom = Mod.Assets.Request<Texture2D>("SpiritMod/Effects/Masks/CircleGradient", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

			//Use a method to cut down on boilerplate with drawing stars
			void DrawStar(Vector2 center, float starSize, float rotation) 
			{
				int bloomstodraw = 3;
				for (int i = 0; i < bloomstodraw; i++)
				{
					float progress = i / (float)bloomstodraw;
					spriteBatch.Draw(Bloom, center - Main.screenPosition, null, Color.HotPink * starSize * MathHelper.Lerp(1f, 0.5f, 1 - progress), 0,
						Bloom.Size() / 2, starSize * 0.4f * MathHelper.Lerp(0.66f, 1f, progress), SpriteEffects.None, 0);
				}

				spriteBatch.Draw(tex, center - Main.screenPosition, null, Color.White * starSize, rotation, tex.Size() / 2, starSize * 0.6f, SpriteEffects.None, 0);
				spriteBatch.Draw(tex, center - Main.screenPosition, null, Color.White * starSize * 0.7f, -rotation, tex.Size() / 2, starSize * 0.5f, SpriteEffects.None, 0);
			}

			float rotationSpeed = 1.5f;
			if (threads.Count == 0) //Fix it otherwise spawning on a thread with only 1 star
				DrawStar(thread2.StartPoint, size, Main.GlobalTimeWrappedHourly * rotationSpeed);

			DrawStar(thread2.EndPoint, size, Main.GlobalTimeWrappedHourly * rotationSpeed);

			//Draw stars at each thread's end and start points
			int threadsDrawn = 0;
			foreach (StarThread thread in threads) 
			{
				size = Math.Min(thread.Counter / THREADGROWLERP, (thread.Duration - thread.Counter) / THREADGROWLERP);
				size = Math.Min(size, 1);
				if (thread.DrawStart || threadsDrawn == 0)
					DrawStar(thread.StartPoint, size, Main.GlobalTimeWrappedHourly * rotationSpeed);

				DrawStar(thread.EndPoint, 1, Main.GlobalTimeWrappedHourly * rotationSpeed);
				if (!thread.DrawStart)
					threadsDrawn++;
			}

			//Draw substars(replace with particles later?)
			foreach (StarThread thread in threads)
			{
				foreach (SubStar star in thread.SubStars)
				{
					float scale = star.MaxScale * (float)Math.Sin(star.Counter / 10f) * 0.5f;
					spriteBatch.Draw(tex, star.Position - Main.screenPosition, null, Color.LightPink, 0, new Vector2(tex.Width, tex.Height) / 2, scale, SpriteEffects.None, 0f);
				}
			}
			foreach (SubStar star in currentThread.SubStars)
			{
				float scale = star.MaxScale * (float)Math.Sin(star.Counter / 10f) * 0.5f;
				spriteBatch.Draw(tex, star.Position - Main.screenPosition, null, Color.LightPink, 0, new Vector2(tex.Width, tex.Height) / 2, scale, SpriteEffects.None, 0f);
			}
		}

		private void UpdateThreads()
		{
			foreach (StarThread thread in threads)
			{
				thread.Update();
			}
			foreach (StarThread thread in threads.ToArray())
			{
				if (thread.Counter > thread.Duration)
					threads.Remove(thread);
			}
			StarThread thread2 = currentThread;
			thread2.Update();
		}
		private void ThreadDeathDust()
		{
			if (Main.dedServ) //Dont do this on server
				return;

			//Burst of star particles at the positions of endpoints
			void StarBreakParticles(Vector2 position)
			{
				for (int i = 0; i < 5; i++)
					Particles.ParticleHandler.SpawnParticle(new Particles.StarParticle(position, Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f),
						Color.LightPink, Color.HotPink, Main.rand.NextFloat(0.2f, 0.3f), 25));
			}

			//Bloom particles along each thread
			void ThreadBreakParticles(StarThread thread, float progress = 1)
			{
				for (int i = 0; i < thread.Length * progress; i += Main.rand.Next(7, 14))
				{
					Vector2 direction = thread.EndPoint - thread.StartPoint;
					direction.Normalize();
					Vector2 position = thread.StartPoint + (direction * i);
					Particles.ParticleHandler.SpawnParticle(new Particles.GlowParticle(position, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f), Color.HotPink,
						Main.rand.NextFloat(0.07f, 0.1f), Main.rand.Next(30, 40), 12));
				}
			}

			foreach (StarThread thread in threads)
			{
				ThreadBreakParticles(thread);
				StarBreakParticles(thread.StartPoint);
				StarBreakParticles(thread.EndPoint);
			}

			ThreadBreakParticles(currentThread, progress);
			StarBreakParticles(currentThread.StartPoint);
			StarBreakParticles(currentThread.EndPoint);
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.AddCommon<Items.Pets.CosmicRattler.CosmicRattler>(15);
	}

	public class StarachnidProj : ModProjectile
	{
		public override void SetStaticDefaults() => DisplayName.SetDefault("Constellation");

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.width = Projectile.height = 38;
			Projectile.timeLeft = 150;
			Projectile.ignoreWater = true;
			Projectile.alpha = 255;
		}

		NPC parent;

		public override void AI()
		{
			parent = Main.npc[(int)Projectile.ai[0]];
			if (parent.active)
			{
				Projectile.Center = parent.Center;
				Projectile.timeLeft = 2;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (parent != null)
			{
				if (parent.active && parent.ModNPC is Starachnid parent2)
				{
					foreach (StarThread thread in parent2.threads)
					{
						if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), thread.StartPoint, thread.EndPoint) && thread.Counter > 35)
							return true;
					}

					StarThread currentThread = parent2.currentThread;
					Vector2 direction = currentThread.EndPoint - currentThread.StartPoint;

					if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), currentThread.StartPoint, currentThread.StartPoint + (direction * parent2.progress)))
						return true;
				}
			}
			return false;
		}
	}
}
