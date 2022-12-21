using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Critters.Algae
{
	public class PurpleAlgae2 : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bioluminescent Algae");
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 6;
			NPC.height = 6;
			NPC.damage = 0;
			NPC.defense = 1000;
			NPC.lifeMax = 1;
			NPC.aiStyle = -1;
			NPC.npcSlots = 0;
			NPC.noGravity = true;
			NPC.alpha = 40;
			NPC.behindTiles = true;
			NPC.dontCountMe = true;
			NPC.dontTakeDamage = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("A tiny purple organism sometimes found floating gently atop of the ocean's waves. It's a wonder why or when they appear."),
			});
		}

		public float num42;
		int num = 0;
		bool collision = false;
		int num1232;

		public override void OnSpawn(IEntitySource source)
		{
			int npcXTile = (int)(NPC.Center.X / 16);
			int npcYTile = (int)(NPC.Center.Y / 16);
			for (int y = npcYTile; y > Math.Max(0, npcYTile - 100); y--)
			{
				if (Main.tile[npcXTile, y].LiquidAmount != 255)
				{
					int liquid = Main.tile[npcXTile, y].LiquidAmount;
					float up = (liquid / 255f) * 16f;
					NPC.position.Y = (y + 1) * 16f - up + 8;
					break;
				}
			}

			if (NPC.type == ModContent.NPCType<PurpleAlgae2>())
			{
				for (int i = 0; i < 5; ++i)
				{
					Vector2 dir = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);
					string[] npcChoices = { "PurpleAlgae1", "PurpleAlgae3" };
					int npcChoice = Main.rand.Next(npcChoices.Length);
					int newNPC = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + (Main.rand.Next(-55, 55)), (int)NPC.Center.Y + (Main.rand.Next(-20, 20)), Mod.Find<ModNPC>(npcChoices[npcChoice]).Type, NPC.whoAmI);
					Main.npc[newNPC].velocity.X = dir.X;
				}
			}
		}

		public override bool PreAI()
		{
			if (Main.dayTime)
			{
				num1232++;
				if (num1232 >= Main.rand.Next(100, 700))
				{
					NPC.active = false;
					NPC.netUpdate = true;
				}
			}
			return true;
		}

		public override void AI()
		{
			if (++num >= Main.rand.Next(100, 400))
				num = 0;

			if (!Main.dayTime)
				Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.208f * 2, 0.107f * 2, .255f * 2);

			NPC.spriteDirection = -NPC.direction;

			if (!collision)
				NPC.velocity.X = .5f * Main.windSpeedCurrent;
			else
				NPC.velocity.X = -.5f * Main.windSpeedCurrent;

			if (NPC.collideX || NPC.collideY)
			{
				NPC.velocity.X *= -1f;
				collision = !collision;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			drawColor = new Color(224 - (int)(num / 3 * 4), 158 - (int)(num / 3 * 4), 255 - (int)(num / 3 * 4), 255 - num);
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			var pos = NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY - 8);

			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, pos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			return false;
		}
	}

	public class PurpleAlgae1 : PurpleAlgae2
	{ 
	}

	public class PurpleAlgae3 : PurpleAlgae2
	{
	}
}