using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace SpiritMod.NPCs.FallingAsteroid
{
	public class Falling_Asteroid : ModNPC
	{
		public int visualTimer = 0;

		private int Counter
		{
			get => (int)NPC.ai[0];
			set => NPC.ai[0] = value;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Falling Asteroid");
			NPCID.Sets.TrailCacheLength[NPC.type] = 30;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
			Main.npcFrameCount[NPC.type] = 5;
			NPCHelper.ImmuneTo(this, BuffID.OnFire, BuffID.Confused, BuffID.Poisoned);
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = 0;
			NPC.lifeMax = 115;
			NPC.defense = 8;
			NPC.value = 350f;
			NPC.knockBackResist = 0f;
			NPC.width = 30;
			NPC.height = 50;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.damage = 30;
			NPC.HitSound = SoundID.NPCHit3;
			NPC.DeathSound = SoundID.NPCDeath43;

			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.FallingAsteroidBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Meteor,
				new FlavorTextBestiaryInfoElement("What appears to be a sentient rock is really a small critter using a chunk of the meteor as a shell. It's able to float around with ease, despite the weight."),
			});
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(BuffID.OnFire, 60 * 3);

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => SpawnCondition.Meteor.Chance * 0.15f;

		public override void AI()
		{
			Player player = Main.player[NPC.target];
			NPC.spriteDirection = 1;

			Counter++;
			if (Counter <= 320 && Counter >= 0)
				Movement();
			else if (Counter > 360)
				Drop();

			if (Counter == 320 || Counter == 360)
				NPC.netUpdate = true;

			if (Counter >= 580)
			{
				Counter = 0;
				NPC.netUpdate = true;
			}

			for (int i = 0; i < 2; ++i)
			{
				Dust dust = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0.0f, 0.0f, 0, new Color(), 1f)];
				dust.position = NPC.Center + Vector2.Normalize(NPC.velocity) / 2f;
				dust.velocity = NPC.velocity.RotatedBy(MathHelper.PiOver2, new Vector2()) * 0.33f + NPC.velocity / 120f;
				dust.position += NPC.velocity.RotatedBy(MathHelper.PiOver2, new Vector2());
				dust.fadeIn = 0.5f;
				dust.noGravity = true;
			}

			if (player.dead)
				NPC.velocity.Y -= 0.15f;
			Lighting.AddLight(NPC.Center, 0.5f, 0.25f, 0f);
		}

		public void Drop()
		{
			NPC.velocity.Y += 0.15f;
			NPC.noTileCollide = false;

			if (NPC.collideY && Counter > 420) //The NPC has impacted
			{
				for (int index = 0; index < 30; ++index)
				{
					Dust dust = Main.dust[Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 50), NPC.width, NPC.height, DustID.Granite, 0.0f, 0.0f, 0, new Color(), 1.2f)];
					dust.velocity.Y -= (float)(3.0 + 2 * 2.5);
					dust.velocity.Y *= Main.rand.NextFloat();
					dust.scale += 8 * 0.03f;
				}

				for (int k = 0; k < 10; k++)
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(NPC.velocity.X * 0.5f, -NPC.velocity.Y * 0.5f), Main.rand.Next(61, 64), 1f);
				for (int k = 0; k < 20; k++)
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, NPC.velocity.X * 2f, -NPC.velocity.Y * 2f, 150, new Color(), 1.2f);

				SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
				Counter = -90;
				NPC.netUpdate = true;

				#region entity collisions
				foreach (NPC npc in Main.npc)
				{
					if (Vector2.Distance(NPC.Center, npc.Center) <= 150f && npc.whoAmI != NPC.whoAmI)
						MoveEntity(npc);
				}
				foreach (Item item in Main.item)
				{
					if (Vector2.Distance(NPC.Center, item.Center) <= 150f)
						MoveEntity(item);
				}
				foreach (Player player in Main.player)
				{
					if (Vector2.Distance(NPC.Center, player.Center) <= 150f)
						MoveEntity(player);
				}
				foreach (Projectile proj in Main.projectile)
				{
					if (Vector2.Distance(NPC.Center, proj.Center) <= 150f && proj.type != ProjectileID.IceBlock)
						MoveEntity(proj);
				}
				#endregion
			}
		}

		public void MoveEntity(Entity entity)
		{
			float num2 = NPC.position.X + Main.rand.Next(-10, 10) + (NPC.width / 2f) - entity.Center.X;
			float num3 = NPC.position.Y + Main.rand.Next(-10, 10) + (NPC.height / 2f) - entity.Center.Y;
			float num4 = 8f / (float)Math.Sqrt(num2 * num2 + num3 * num3);
			entity.velocity.X = num2 * num4 * -1.7f;
			entity.velocity.Y = num3 * num4 * -1.7f;
		}

		public void Movement()
		{
			const float MoveSpeed = 0.35f;

			Player player = Main.player[NPC.target];

			NPC.noTileCollide = true;
			float number3 = Main.player[NPC.target].Center.X - NPC.Center.X;
			float number4 = (float)(Main.player[NPC.target].Center.Y - NPC.Center.Y - 300.0);
			float num5 = (float)Math.Sqrt(number3 * number3 + number4 * number4);

			if (player == Main.player[NPC.target])
			{
				float num6 = NPC.velocity.X;
				float num7 = NPC.velocity.Y;

				if (num5 >= 20)
				{
					float num8 = 4f / num5;
					num6 = number3 * num8;
					num7 = number4 * num8;
				}

				if (NPC.velocity.X < num6)
				{
					NPC.velocity.X += MoveSpeed;
					if (NPC.velocity.X < 0 && num6 > 0)
						NPC.velocity.X += MoveSpeed * 2f;
				}
				else if (NPC.velocity.X > num6)
				{
					NPC.velocity.X -= MoveSpeed;
					if (NPC.velocity.X > 0 && num6 < 0)
						NPC.velocity.X -= MoveSpeed * 2f;
				}
				if (NPC.velocity.Y < num7)
				{
					NPC.velocity.Y += MoveSpeed;
					if (NPC.velocity.Y < 0 && num7 > 0)
						NPC.velocity.Y += MoveSpeed * 2f;
				}
				else if (NPC.velocity.Y > num7)
				{
					NPC.velocity.Y -= MoveSpeed;
					if (NPC.velocity.Y > 0 && num7 < 0)
						NPC.velocity.Y -= MoveSpeed * 2f;
				}
			}

			if (++NPC.ai[1] >= 5f)
			{
				NPC.ai[1] = 0f;
				NPC.netUpdate = true;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			Player player = Main.player[NPC.target];

			if (player.active || NPC.IsABestiaryIconDummy)
			{
				NPC.frameCounter++;
				if (NPC.frameCounter < 6)
					NPC.frame.Y = 0 * frameHeight;
				else if (NPC.frameCounter < 12)
					NPC.frame.Y = 1 * frameHeight;
				else if (NPC.frameCounter < 18)
					NPC.frame.Y = 2 * frameHeight;
				else if (NPC.frameCounter < 24)
					NPC.frame.Y = 3 * frameHeight;
				else if (NPC.frameCounter < 28)
					NPC.frame.Y = 4 * frameHeight;
				else
					NPC.frameCounter = 0;
			}
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
				for (int i = 1; i < 5; ++i)
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("FallenAsteroidGore" + i).Type, 1f);

			for (int k = 0; k < 7; k++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 2.5f * hitDirection, -2.5f, 0, default, 1.2f);
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 2.5f * hitDirection, -2.5f, 0, default, 0.5f);
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 2.5f * hitDirection, -2.5f, 0, default, 0.7f);
			}
		}

		//public override void OnKill()
		//{
		//	if (QuestManager.GetQuest<Mechanics.QuestSystem.Quests.StylistQuestMeteor>().IsActive && Main.rand.NextBool(3))
		//		Item.NewItem(npc.Center, ModContent.ItemType<Items.Sets.MaterialsMisc.QuestItems.MeteorDyeMaterial>());
		//}

		public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ItemDropRule.Common(116, 10, 2, 4));

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Counter > 360 || NPC.IsABestiaryIconDummy)
			{
				++visualTimer;

				Player player = Main.player[NPC.target];

				bool flag2 = Vector2.Distance(NPC.Center, player.Center) > 0f && NPC.Center.Y == player.Center.Y;
				if (visualTimer >= 30f && flag2)
					visualTimer = 0;

				SpriteEffects spriteEffects = SpriteEffects.None;
				float addHeight = -4f;
				var vector2_3 = new Vector2(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);

				if (NPC.velocity.X == 0)
					addHeight = 0f;

				Texture2D tex = TextureAssets.Extra[55].Value;
				Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 8 + 14);

				float num2 = -MathHelper.PiOver2 * NPC.rotation;
				float amount = visualTimer / 45f;
				if (amount > 1f)
					amount = 1f;

				for (int index = 5; index >= 0; --index)
				{
					var color2 = Color.Lerp(Color.Lerp(Color.Gold, Color.OrangeRed, amount), Color.Blue, index / 12f);

					color2.A = (byte)(64.0 * amount);
					color2.R = (byte)(color2.R * (10 - index) / 20);
					color2.G = (byte)(color2.G * (10 - index) / 20);
					color2.B = (byte)(color2.B * (10 - index) / 20);
					color2.A = (byte)(color2.A * (10 - index) / 20);
					color2 *= amount;

					int frameY = ((visualTimer / (NPC.IsABestiaryIconDummy ? 6 : 2) % 4) - index) % 4;
					if (frameY < 0)
						frameY += 4;

					Rectangle rectangle = tex.Frame(1, 4, 0, frameY);

					var pos = new Vector2(NPC.oldPos[index].X + (NPC.width / 2f) - TextureAssets.Npc[NPC.type].Value.Width * NPC.scale / 2 + vector2_3.X - .5f, NPC.oldPos[index].Y + NPC.height - TextureAssets.Npc[NPC.type].Value.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4 + vector2_3.Y + addHeight) * NPC.scale;

					Main.EntitySpriteDraw(tex, pos - screenPos, rectangle, color2, num2, origin, MathHelper.Lerp(0.1f, 1.2f, (10 - index) / 15f), spriteEffects, 0);
				}
			}
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			GlowmaskUtils.DrawNPCGlowMask(spriteBatch, NPC, Mod.Assets.Request<Texture2D>("NPCs/FallingAsteroid/Falling_Asteroid_Glow").Value, screenPos);
		}
	}
}