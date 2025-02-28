using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SpiritMod.Items.Sets.RlyehianDrops
{
	public class TentacleChainProj : ModProjectile
	{
		private int Counter
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private float firingSpeed;
		private float initialRotation;

		public override void SetStaticDefaults() => DisplayName.SetDefault("Brine Barrage");

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ownerHitCheck = true;
		}

		// This projectile uses advanced calculation for its motion.
		public override void AI()
		{
			// Face the projectile towards its movement direction, offset by 90 degrees counterclockwise because the sprite faces downward.
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(-90f);

			Player player = Main.player[Projectile.owner];
			if (Counter == 0)
			{
				firingSpeed = player.HeldItem.shootSpeed;
				initialRotation = Projectile.rotation;
			}
			int firingTime = player.itemTimeMax * 3;
			int firingAnim = player.itemAnimationMax * 3;

			// Constantly set the chain's timeLeft to 2 so that it doesn't die.
			Projectile.spriteDirection = Projectile.direction;
			Projectile.timeLeft = 2;

			// If localAI[0] (the localAI slot we use to store initial rotation)'s X value is greater than 0, then direction is 1. Otherwise, -1.
			float direction = (initialRotation.ToRotationVector2().X >= 0f).ToDirectionInt();

			// Use a sine calculation to rotate the Solar Eruption around to form an ovular motion.
			Vector2 rotation = (direction * (Projectile.ai[0] / firingAnim * MathHelper.ToRadians(360f) + MathHelper.ToRadians(-90f))).ToRotationVector2();
			rotation.Y *= (float)Math.Sin(Projectile.ai[1]);

			rotation = rotation.RotatedBy(initialRotation);

			// Use the ai[0] slot as a timer to increment how long the projectile has been alive.
			Projectile.ai[0] += 1f;
			if (++Projectile.ai[0] < firingTime)
			{
				Projectile.velocity += (firingSpeed * rotation).RotatedBy(MathHelper.ToRadians(90f));
			}
			else
			{
				// If past the firingTime variable we set in the item's Shoot() hook, kill it.
				Projectile.Kill();
			}

			Vector2 offset = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
			// Flip the offset horizontally if the player is facing left instead of right.
			if (player.direction == -1)
				offset.X = player.bodyFrame.Width - offset.X;
			if (player.gravDir == -1f)
				offset.Y = player.bodyFrame.Height - offset.Y;

			// This line is a custom offset that you can change to move the handle around. Default is 0f, 0f. This projectile uses 4f, -6f.
			offset += new Vector2(4f, -6f) * new Vector2(player.direction, player.gravDir);
			offset -= new Vector2(player.bodyFrame.Width - Projectile.width, player.bodyFrame.Height - 42) * 0.5f;
			Projectile.Center = player.RotatedRelativePoint(player.position + offset) - Projectile.velocity;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => Projectile.localNPCImmunity[target.whoAmI] = target.immune[Projectile.owner] = 10;

		public override bool? CanCutTiles() => true;

		public override void CutTiles()
		{
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity, (Projectile.width + Projectile.height) * 0.5f * Projectile.scale, DelegateMethods.CutTiles);
		}

		// Plot a line from the start of the Solar Eruption to the end of it, and check if any hitboxes are intersected by it for the entity collision logic. (Don't change this.)
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity, (Projectile.width + Projectile.height) * 0.5f * Projectile.scale, ref collisionPoint);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Color color = lightColor;

			// Some rectangle presets for different parts of the chain.
			Rectangle chainHandle = new Rectangle(0, 40, texture.Width, 24);
			Rectangle chainLinkEnd = new Rectangle(0, 0, texture.Width, 12);
			Rectangle chainLink = new Rectangle(0, 0, texture.Width, 12);
			Rectangle chainHead = new Rectangle(0, 16, texture.Width, 24);

			// If the chain isn't moving, stop drawing all of its components.
			if (Projectile.velocity == Vector2.Zero)
				return false;

			// These fields / pre-draw logic have been taken from the vanilla source code for the Solar Eruption.
			// They setup distances, directions, offsets, and rotations all so the chain faces correctly.
			float chainDistance = Projectile.velocity.Length() + 16f;
			bool distanceCheck = chainDistance < 100f;
			Vector2 direction = Vector2.Normalize(Projectile.velocity);
			Rectangle rectangle = chainHandle;
			Vector2 yOffset = new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
			float rotation = direction.ToRotation() + MathHelper.ToRadians(-90f);
			// Draw the chain handle. This is the first piece in the sprite.
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + yOffset, rectangle, color, rotation, rectangle.Size() / 2f - Vector2.UnitY * 4f, Projectile.scale, SpriteEffects.None, 0);
			chainDistance -= 40f * Projectile.scale;
			Vector2 position = Projectile.Center;
			position += direction * Projectile.scale * 12f;
			rectangle = chainLinkEnd;
			if (chainDistance > 0f)
			{
				float chains = 0f;
				while (chains + 1f < chainDistance)
				{
					if (chainDistance - chains < rectangle.Height)
					{
						rectangle.Height = (int)(chainDistance - chains);
					}
					// Draws the chain links between the handle and the head. This is the "line," or the third piece in the sprite.
					Main.EntitySpriteDraw(texture, position - Main.screenPosition + yOffset, rectangle, Lighting.GetColor((int)position.X / 16, (int)position.Y / 16), rotation, new Vector2(rectangle.Width / 2, 0f), Projectile.scale, SpriteEffects.None, 0);
					chains += rectangle.Height * Projectile.scale;
					position += direction * rectangle.Height * Projectile.scale;
				}
			}
			Vector2 chainEnd = position;
			position = Projectile.Center;
			position += direction * Projectile.scale * 24f;
			rectangle = chainLink;
			int offset = distanceCheck ? 9 : 18;
			float chainLinkDistance = chainDistance;
			if (chainDistance > 0f)
			{
				float chains = 0f;
				float increment = chainLinkDistance / offset;
				chains += increment * 0.25f;
				position += direction * increment * 0.25f;
				for (int i = 0; i < offset; i++)
				{
					float spacing = increment;
					if (i == 0)
					{
						spacing *= 0.75f;
					}
					// Draws the actual chain link spikes between the handle and the head. These are the "spikes," or the second piece in the sprite.
					Main.EntitySpriteDraw(texture, position - Main.screenPosition + yOffset, rectangle, Lighting.GetColor((int)position.X / 16, (int)position.Y / 16), rotation, new Vector2(rectangle.Width / 2, 0f), Projectile.scale, SpriteEffects.None, 0);
					chains += spacing;
					position += direction * spacing;
				}
			}
			rectangle = chainHead;
			// Draw the chain head. This is the fourth piece in the sprite.
			Main.EntitySpriteDraw(texture, chainEnd - Main.screenPosition + yOffset, rectangle, Lighting.GetColor((int)chainEnd.X / 16, (int)chainEnd.Y / 16), rotation, texture.Frame().Top(), Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}