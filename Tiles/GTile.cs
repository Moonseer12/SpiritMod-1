﻿using SpiritMod.Items.Material;
using SpiritMod.Items.Weapon.Thrown;
using SpiritMod.NPCs.OceanSlime;
using SpiritMod.Items.Consumable.Food;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using SpiritMod.Tiles.Vanilla;

namespace SpiritMod.Tiles
{
	public class GTile : GlobalTile
	{
		internal static readonly int[] DirtAndDecor = { TileID.Dirt, TileID.Plants, TileID.SmallPiles, TileID.LargePiles, TileID.LargePiles2, TileID.MushroomPlants, TileID.Pots };

		public override void RandomUpdate(int i, int j, int type)
		{
			if (type == TileID.Cloud || type == TileID.RainCloud || type == TileID.SnowCloud)
				CloudRandomUpdate.OnTick(i, j, type);

			if (type == TileID.Pearlstone)
				PearlstoneRandomUpdate.OnTick(i, j);

			if (type == TileID.CorruptGrass || type == TileID.Ebonstone)
				CorpsebloomRandomUpdate.OnTick(i, j);
		}

		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (!Main.dedServ)
			{
				Player player = Main.LocalPlayer;
				MyPlayer modPlayer = player.GetSpiritPlayer();

				if (type == TileID.Stone || type == 25 || type == 117 || type == 203 || type == 57)
				{
					bool struckThisTile = player.ItemAnimationActive && ((new Vector2((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)) == new Vector2(i, j)) || (Main.SmartCursorIsUsed && new Vector2(Main.SmartCursorX, Main.SmartCursorY) == new Vector2(i, j)));
					
					if (modPlayer.gemPickaxe && struckThisTile && !fail)
					{
						int tremorItem = Main.rand.Next(new int[] { 11, 12, 13, 14, 699, 700, 701, 702, 999, 182, 178, 179, 177, 180, 181 });
						if (Main.hardMode)
							tremorItem = Main.rand.Next(new int[] { 11, 12, 13, 14, 699, 700, 701, 702, 999, 182, 178, 179, 177, 180, 181, 364, 365, 366, 1104, 1105, 1106 });

						SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/PositiveOutcome"), new Vector2(i * 16, j * 16));

						int id = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, tremorItem, Main.rand.Next(1, 3));

						if (Main.netMode != NetmodeID.SinglePlayer)
							NetMessage.SendData(MessageID.SyncItem, number: id);
					}
				}

				if (player.GetSpiritPlayer().wayfarerSet && type == 28)
					player.AddBuff(ModContent.BuffType<Buffs.Armor.ExplorerPot>(), 360);
				if (player.GetSpiritPlayer().wayfarerSet && Main.tileSpelunker[type] && Main.tileSolid[type])
					player.AddBuff(ModContent.BuffType<Buffs.Armor.ExplorerMine>(), 600);

				if (player.cordage && type == ModContent.TileType<Ambient.Briar.BriarVines>())
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 48, ItemID.VineRope);
				if (Main.rand.NextBool(40) && type == ModContent.TileType<Ambient.HangingChimes.HangingChimes>())
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 48, ItemID.CrystalShard);
				if (player.inventory[player.selectedItem].type == ItemID.Sickle && (type == ModContent.TileType<Ambient.Briar.BriarFoliage>() || type == ModContent.TileType<Tiles.Ambient.Briar.BriarFoliage1>()))
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 48, ItemID.Hay);
			}
		}

		public override void FloorVisuals(int type, Player player)
		{
			foreach (var effect in player.GetSpiritPlayer().effects)
				effect.TileFloorVisuals(type, player);

			if (type == TileID.Sand && player.GetSpiritPlayer().scarabCharm)
				player.jumpSpeedBoost += .15f;
		}

		public override bool Drop(int i, int j, int type)
		{
			if (!Main.dedServ)
			{
				Player player = Main.LocalPlayer;
				if (type == TileID.PalmTree && Main.rand.NextBool(3) && player.ZoneBeach)
				{
					if (Main.rand.NextBool(2))
						Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 48, ModContent.ItemType<Coconut>(), Main.rand.Next(5, 8));
					if (NPC.CountNPCS(ModContent.NPCType<OceanSlime>()) < 1)
						NPC.NewNPC(new EntitySource_TileBreak(i, j), i * 16, (j - 10) * 16, ModContent.NPCType<OceanSlime>(), 0, 0.0f, -8.5f, 0.0f, 0.0f, byte.MaxValue);
				}

				if (type == 72)
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 48, ModContent.ItemType<GlowRoot>(), Main.rand.Next(0, 2));
				if (type == TileID.Trees && Main.rand.NextBool(25) && player.ZoneSnow && Main.rand.NextBool(4))
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 48, ModContent.ItemType<IceBerries>(), Main.rand.Next(1, 3));

				if ((type == TileID.MatureHerbs || type == TileID.BloomingHerbs) && player.GetModPlayer<Items.Armor.BotanistSet.BotanistPlayer>().active)
					HerbBotanistCompat.BotanistDrops(i, j);
			}
			return true;
		}

		/// <summary>
		/// Code from Vortex, thanks for slogging through vanilla to get this!
		/// </summary>
		public static void DrawSlopedGlowMask(int i, int j, Texture2D texture, Color drawColor, Vector2 positionOffset, bool overrideFrame = false)
		{
			Tile tile = Main.tile[i, j];
			int frameX = tile.TileFrameX;
			int frameY = tile.TileFrameY;

			if (overrideFrame)
			{
				frameX = 0;
				frameY = 0;
			}

			int width = 16;
			int height = 16;
			Vector2 location = new Vector2(i * 16, j * 16);
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
			Vector2 offsets = -Main.screenPosition + zero + positionOffset;
			Vector2 drawCoordinates = location + offsets;

			if ((tile.Slope == 0 && !tile.IsHalfBlock) || (Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType])) //second one should be for platforms
				Main.spriteBatch.Draw(texture, drawCoordinates, new Rectangle(frameX, frameY, width, height), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			else if (tile.IsHalfBlock)
				Main.spriteBatch.Draw(texture, new Vector2(drawCoordinates.X, drawCoordinates.Y + 8), new Rectangle(frameX, frameY, width, 8), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			else
			{
				byte b = (byte)tile.Slope;
				Rectangle frame;
				Vector2 drawPos;

				if (b == 1 || b == 2)
				{
					int length;
					int height2;

					for (int a = 0; a < 8; ++a)
					{
						if (b == 2)
						{
							length = 16 - a * 2 - 2;
							height2 = 14 - a * 2;
						}
						else
						{
							length = a * 2;
							height2 = 14 - length;
						}

						frame = new Rectangle(frameX + length, frameY, 2, height2);
						drawPos = new Vector2(i * 16 + length, j * 16 + a * 2) + offsets;
						Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
					}

					frame = new Rectangle(frameX, frameY + 14, 16, 2);
					drawPos = new Vector2(i * 16, j * 16 + 14) + offsets;
					Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
				}
				else
				{
					int length;
					int height2;

					for (int a = 0; a < 8; ++a)
					{
						if (b == 3)
						{
							length = a * 2;
							height2 = 16 - length;
						}
						else
						{
							length = 16 - a * 2 - 2;
							height2 = 16 - a * 2;
						}

						frame = new Rectangle(frameX + length, frameY + 16 - height2, 2, height2);
						drawPos = new Vector2(i * 16 + length, j * 16) + offsets;
						Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
					}

					drawPos = new Vector2(i * 16, j * 16) + offsets;
					frame = new Rectangle(frameX, frameY, 16, 2);
					Main.spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
				}
			}
		}
	}
}