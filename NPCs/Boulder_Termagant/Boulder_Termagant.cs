using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;

namespace SpiritMod.NPCs.Boulder_Termagant
{
	public class Boulder_Termagant : ModNPC
	{
		public bool hasGottenColor = false;
		public bool resetFrames = false;
		public bool isRoaring = false;
		public int r = 0;
		public int g = 0;
		public int b = 0;
		public int randomColor = 0;
		public int boulderTimer = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Boulder Behemoth");
			Main.npcFrameCount[NPC.type] = 11;
			NPCHelper.ImmuneTo(this, BuffID.Poisoned);

			NPCID.Sets.TrailCacheLength[NPC.type] = 30;
			NPCID.Sets.TrailingMode[NPC.type] = 0;

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { PortraitPositionYOverride = -20 };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = 3;
			NPC.lifeMax = 365;
			NPC.defense = 28;
			NPC.value = 1250f;
			AIType = 0;
			NPC.knockBackResist = 0.1f;
			NPC.width = 50;
			NPC.height = 38;
			NPC.damage = 70;
			NPC.lavaImmune = false;
			NPC.noTileCollide = false;
			NPC.HitSound = SoundID.NPCHit6;
			NPC.DeathSound = SoundID.NPCDeath5;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.BoulderBehemothBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				new FlavorTextBestiaryInfoElement("Entranced by anything that glistens, this beast's eyes now serve only one purpose. Find any glint of light. This usually leads it to gems, but it will mistakenly eat anything that glows."),
			});
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(hasGottenColor);
			writer.Write(resetFrames);
			writer.Write(isRoaring);
			writer.Write(r);
			writer.Write(g);
			writer.Write(b);
			writer.Write(randomColor);
			writer.Write(boulderTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			hasGottenColor = reader.ReadBoolean();
			resetFrames = reader.ReadBoolean();
			isRoaring = reader.ReadBoolean();

			r = reader.ReadInt32();
			g = reader.ReadInt32();
			b = reader.ReadInt32();

			randomColor = reader.ReadInt32();
			boulderTimer = reader.ReadInt32();
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			if (projectile.type != ProjectileID.Boulder && projectile.type != ProjectileID.BoulderStaffOfEarth)
				return base.CanBeHitByProjectile(projectile);
			return false;
		}

		public override void AI()
		{
			Player player = Main.player[NPC.target];

			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;

			if (!hasGottenColor)
			{
				hasGottenColor = true;
				randomColor = Main.rand.Next(6);
				if (randomColor == 0)
				{
					r = 219;
					g = 97;
					b = 255;
				}
				if (randomColor == 1)
				{
					r = 255;
					g = 198;
					b = 0;
				}
				if (randomColor == 2)
				{
					r = 23;
					g = 147;
					b = 234;
				}
				if (randomColor == 3)
				{
					r = 33;
					g = 184;
					b = 115;
				}
				if (randomColor == 4)
				{
					r = 238;
					g = 51;
					b = 53;
				}
				if (randomColor == 5)
				{
					r = 223;
					g = 230;
					b = 238;
				}
			}
			Lighting.AddLight(NPC.Center, r * 0.002f, g * 0.002f, b * 0.002f);

			if (Vector2.Distance(NPC.Center, player.Center) < 800f && Collision.CanHitLine(NPC.Center, 0, 0, Main.player[NPC.target].Center, 0, 0))
				boulderTimer++;

			if (boulderTimer > 299 && boulderTimer < 421)
			{
				if (boulderTimer == 300)
				{
					SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				}
				NPC.aiStyle = -1;
				NPC.velocity.X = 0f;
				isRoaring = true;
				NPC.defense = 60;
				if (!resetFrames)
				{
					NPC.netUpdate = true;
					NPC.frameCounter = 0;
					resetFrames = true;
				}
				if (boulderTimer == 420)
				{
					NPC.netUpdate = true;
					boulderTimer = 0;
					resetFrames = false;
					isRoaring = false;
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 5; i++)
						{
							int proj;

							if (player.ZoneGranite)
								proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center.X - Main.rand.Next(-300, 300), player.Center.Y - Main.rand.Next(800, 1200), 0f, 2f + Main.rand.Next(1, 3), ModContent.ProjectileType<Granite_Boulder>(), 15, 0, Main.myPlayer, 0, 0);
							else if (player.ZoneMarble)
								proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center.X - Main.rand.Next(-300, 300), player.Center.Y - Main.rand.Next(800, 1200), 0f, 2f + Main.rand.Next(1, 3), ModContent.ProjectileType<Marble_Boulder>(), 15, 0, Main.myPlayer, 0, 0);
							else
								proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center.X - Main.rand.Next(-300, 300), player.Center.Y - Main.rand.Next(800, 1200), 0f, 2f + Main.rand.Next(1, 3), ModContent.ProjectileType<Cavern_Boulder>(), 15, 0, Main.myPlayer, 0, 0);

							Main.projectile[proj].netUpdate = true;
						}
					}
				}
			}
			else
			{
				NPC.aiStyle = 3;
				NPC.defense = 28;
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.StoneBlock, 1, 10, 25));

			LeadingConditionRule r219Rule = new LeadingConditionRule(new DropRuleConditions.NPCConditional("", (npc) => npc.ModNPC is Boulder_Termagant bold && bold.r == 219));
			r219Rule.OnSuccess(ItemDropRule.Common(ItemID.Amethyst, 1, 1, 4));

			LeadingConditionRule r255Rule = new LeadingConditionRule(new DropRuleConditions.NPCConditional("", (npc) => npc.ModNPC is Boulder_Termagant bold && bold.r == 255));
			r255Rule.OnSuccess(ItemDropRule.Common(ItemID.Topaz, 1, 1, 4));

			LeadingConditionRule r23Rule = new LeadingConditionRule(new DropRuleConditions.NPCConditional("", (npc) => npc.ModNPC is Boulder_Termagant bold && bold.r == 23));
			r23Rule.OnSuccess(ItemDropRule.Common(ItemID.Sapphire, 1, 1, 4));

			LeadingConditionRule r33Rule = new LeadingConditionRule(new DropRuleConditions.NPCConditional("", (npc) => npc.ModNPC is Boulder_Termagant bold && bold.r == 33));
			r33Rule.OnSuccess(ItemDropRule.Common(ItemID.Emerald, 1, 1, 4));

			LeadingConditionRule r238Rule = new LeadingConditionRule(new DropRuleConditions.NPCConditional("", (npc) => npc.ModNPC is Boulder_Termagant bold && bold.r == 238));
			r238Rule.OnSuccess(ItemDropRule.Common(ItemID.Ruby, 1, 1, 4));

			LeadingConditionRule r223Rule = new LeadingConditionRule(new DropRuleConditions.NPCConditional("", (npc) => npc.ModNPC is Boulder_Termagant bold && bold.r == 223));
			r223Rule.OnSuccess(ItemDropRule.Common(ItemID.Diamond, 1, 1, 4));

			npcLoot.Add(r219Rule, r255Rule, r23Rule, r33Rule, r238Rule, r223Rule);
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RockTermagantGore4").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RockTermagantGore3").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RockTermagantGore2").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RockTermagantGore1").Type, 1f);
			}
			for (int k = 0; k < 7; k++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, 2.5f * hitDirection, -2.5f, 0, default, 1.2f);
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, 2.5f * hitDirection, -2.5f, 0, default, 0.5f);
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, 2.5f * hitDirection, -2.5f, 0, default, 0.7f);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => !spawnInfo.PlayerSafe && spawnInfo.SpawnTileY > Main.rockLayer && Main.hardMode && !(spawnInfo.Player.ZoneDungeon || spawnInfo.Player.ZoneSnow || spawnInfo.Player.ZoneJungle) && !spawnInfo.Player.ZoneUnderworldHeight ? 0.045f : 0f;

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 vector2_3 = new Vector2((float)(TextureAssets.Npc[NPC.type].Value.Width / 2), (float)(TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2));
			float addHeight = 4f;
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Bottom - screenPos + new Vector2((float)(-TextureAssets.Npc[NPC.type].Value.Width * NPC.scale / 2.0 + vector2_3.X * (double)NPC.scale), (float)(-TextureAssets.Npc[NPC.type].Value.Height * (double)NPC.scale / Main.npcFrameCount[NPC.type] + 4.0 + vector2_3.Y * NPC.scale) + addHeight + NPC.gfxOffY), NPC.frame,
				NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			Main.spriteBatch.Draw(Mod.Assets.Request<Texture2D>("NPCs/Boulder_Termagant/Boulder_Termagant_Glow").Value, NPC.Bottom - screenPos + new Vector2((float)(-TextureAssets.Npc[NPC.type].Value.Width * NPC.scale / 2.0 + vector2_3.X * (double)NPC.scale), (-TextureAssets.Npc[NPC.type].Value.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4.0f + vector2_3.Y * NPC.scale) + addHeight + NPC.gfxOffY), NPC.frame, new Color(r - NPC.alpha, byte.MaxValue - NPC.alpha, g - NPC.alpha, b - NPC.alpha), NPC.rotation, vector2_3, NPC.scale, effects, 0.0f);

			const int Repeats = 8;

			for (int index = 0; index < Repeats; ++index)
			{
				Vector2 rot = new Vector2(1.4f).RotatedBy(MathHelper.TwoPi * (index / (float)Repeats));
				Main.spriteBatch.Draw(Mod.Assets.Request<Texture2D>("NPCs/Boulder_Termagant/Boulder_Termagant_Glow").Value, NPC.Bottom - screenPos + new Vector2((float)(-TextureAssets.Npc[NPC.type].Value.Width * NPC.scale / 2.0 + vector2_3.X * NPC.scale), (-TextureAssets.Npc[NPC.type].Value.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4.0f + vector2_3.Y * NPC.scale) + addHeight + NPC.gfxOffY) + rot, NPC.frame, new Color(r, g, b, 0), NPC.rotation, vector2_3, NPC.scale, effects, 0.0f);
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (!isRoaring)
			{
				if (NPC.velocity.Y == 0f || NPC.IsABestiaryIconDummy)
				{
					if (NPC.velocity.X != 0f || NPC.IsABestiaryIconDummy)
					{
						if (NPC.frameCounter < 6)
							NPC.frame.Y = 0 * frameHeight;
						else if (NPC.frameCounter < 12)
							NPC.frame.Y = 1 * frameHeight;
						else if (NPC.frameCounter < 18)
							NPC.frame.Y = 2 * frameHeight;
						else if (NPC.frameCounter < 24)
							NPC.frame.Y = 3 * frameHeight;
						else if (NPC.frameCounter < 30)
							NPC.frame.Y = 4 * frameHeight;
						else if (NPC.frameCounter < 36)
							NPC.frame.Y = 5 * frameHeight;
						else if (NPC.frameCounter < 42)
							NPC.frame.Y = 6 * frameHeight;
						else
							NPC.frameCounter = 0;
					}
				}
				else
					NPC.frame.Y = 4 * frameHeight;
			}
			else
			{
				if (NPC.frameCounter < 15)
					NPC.frame.Y = 7 * frameHeight;
				else if (NPC.frameCounter < 30)
					NPC.frame.Y = 8 * frameHeight;
				else if (NPC.frameCounter < 45)
					NPC.frame.Y = 9 * frameHeight;
				else if (NPC.frameCounter < 120)
					NPC.frame.Y = 10 * frameHeight;
				else
					NPC.frameCounter = 0;
			}
		}
	}
}