﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod
{
	public static class GlowmaskUtils
	{
		public static void DrawNPCGlowMask(SpriteBatch spriteBatch, NPC npc, Texture2D texture, Vector2 screenPos, Color? color = null)
		{
			var effects = npc.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				texture,
				npc.Center - screenPos + new Vector2(0, npc.gfxOffY),
				npc.frame,
				npc.GetNPCColorTintedByBuffs(color ?? Color.White),
				npc.rotation,
				npc.frame.Size() / 2,
				npc.scale,
				effects,
				0
			);
		}

		public static void DrawExtras(SpriteBatch spriteBatch, NPC npc, Texture2D texture)
		{
			var effects = npc.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(
				texture,
				npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY),
				npc.frame,
				new Color(200, 200, 200),
				npc.velocity.X * .1f,
				npc.frame.Size() / 2,
				npc.scale,
				effects,
				0
			);
		}

		public static void DrawArmorGlowMask(EquipType type, Texture2D texture, PlayerDrawSet info)
		{
			switch (type)
			{
				case EquipType.Head:
					{
						Vector2 adjustedPosition = new Vector2((int)(info.Position.X - Main.screenPosition.X) + ((info.drawPlayer.width - info.drawPlayer.bodyFrame.Width) / 2), (int)(info.Position.Y - Main.screenPosition.Y) + info.drawPlayer.height - info.drawPlayer.bodyFrame.Height + 4);
						
						DrawData drawData = new DrawData(texture, adjustedPosition + info.drawPlayer.headPosition + info.rotationOrigin, info.drawPlayer.bodyFrame, info.headGlowColor, info.drawPlayer.headRotation, info.rotationOrigin, 1f, info.playerEffect, 0)
						{
							shader = info.cHead
						};
						info.DrawDataCache.Add(drawData);
					}
					return;

				case EquipType.Body:
					{
						var bodyFrame = info.compTorsoFrame;

						if (!info.drawPlayer.invis)
						{
							Vector2 adjustedPos = info.Position - new Vector2((info.compTorsoFrame.Width / 2) + (info.drawPlayer.width / 2), info.drawPlayer.height) - Main.screenPosition;
							Vector2 realPos = adjustedPos + info.drawPlayer.bodyPosition + (info.compTorsoFrame.Size() / 2f).ToPoint().ToVector2();

							DrawData drawData = new DrawData(texture, realPos, bodyFrame, info.bodyGlowColor, info.drawPlayer.bodyRotation, info.rotationOrigin, 1f, info.playerEffect, 0)
							{
								shader = info.cBody
							};
							info.DrawDataCache.Add(drawData);
						}
					}
					return;

				case EquipType.Legs:
					{
						if (info.drawPlayer.shoe != 15 || info.drawPlayer.wearsRobe)
						{
							if (!info.drawPlayer.invis)
							{
								Vector2 adjPos = new Vector2((int)(info.Position.X - Main.screenPosition.X - (info.drawPlayer.legFrame.Width / 2) + (info.drawPlayer.width / 2)), (int)(info.Position.Y - Main.screenPosition.Y + info.drawPlayer.height - info.drawPlayer.legFrame.Height + 4));
								DrawData drawData = new DrawData(texture, adjPos + info.drawPlayer.legPosition + info.rotationOrigin, info.drawPlayer.legFrame, info.legsGlowColor, info.drawPlayer.legRotation, info.rotationOrigin, 1f, info.playerEffect, 0)
								{
									shader = info.cLegs
								};
								info.DrawDataCache.Add(drawData);
							}
						}
					}
					return;
			}
		}

		public static void DrawItemGlowMask(Texture2D texture, PlayerDrawSet info)
		{
			Item item = info.drawPlayer.HeldItem;
			if (info.shadow != 0f || info.drawPlayer.frozen || ((info.drawPlayer.itemAnimation <= 0 || item.useStyle == ItemUseStyleID.None) && (item.holdStyle <= 0 || info.drawPlayer.pulley)) || info.drawPlayer.dead || item.noUseGraphic || (info.drawPlayer.wet && item.noWet))
				return;

			Vector2 offset = Vector2.Zero;
			Vector2 origin = Vector2.Zero;
			float rotOffset = 0;

			if (item.useStyle == ItemUseStyleID.Shoot)
			{
				if (Item.staff[item.type])
				{
					rotOffset = 0.785f * info.drawPlayer.direction;
					if (info.drawPlayer.gravDir == -1f)
						rotOffset -= 1.57f * info.drawPlayer.direction;

					origin = new Vector2(texture.Width * 0.5f * (1 - info.drawPlayer.direction), (info.drawPlayer.gravDir == -1f) ? 0 : texture.Height);

					int oldOriginX = -(int)origin.X;
					ItemLoader.HoldoutOrigin(info.drawPlayer, ref origin);
					offset = new Vector2(origin.X + oldOriginX, 0);
				}
				else
				{
					offset = new Vector2(10, texture.Height / 2);
					ItemLoader.HoldoutOffset(info.drawPlayer.gravDir, item.type, ref offset);
					origin = new Vector2(-offset.X, texture.Height / 2);
					if (info.drawPlayer.direction == -1)
						origin.X = texture.Width + offset.X;

					offset = new Vector2(texture.Width / 2, offset.Y);
				}
			}
			else
			{
				origin = new Vector2(texture.Width * 0.5f * (1 - info.drawPlayer.direction), (info.drawPlayer.gravDir == -1f) ? 0 : texture.Height);
			}

			info.DrawDataCache.Add(new DrawData(
				texture,
				info.ItemLocation - Main.screenPosition + offset,
				texture.Bounds,
				Color.White * ((255f - item.alpha) / 255f),
				info.drawPlayer.itemRotation + rotOffset,
				origin,
				item.scale,
				info.playerEffect,
				0
			));
		}

		public static void DrawItemGlowMaskWorld(SpriteBatch spriteBatch, Item item, Texture2D texture, float rotation, float scale)
		{
			Main.spriteBatch.Draw(
				texture,
				new Vector2(item.position.X - Main.screenPosition.X + item.width / 2, item.position.Y - Main.screenPosition.Y + item.height - (texture.Height / 2)),
				new Rectangle(0, 0, texture.Width, texture.Height),
				Color.White * ((255f - item.alpha) / 255f),
				rotation,
				new Vector2(texture.Width / 2, texture.Height / 2),
				scale,
				SpriteEffects.None,
				0f
			);
		}
	}
}