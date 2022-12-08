using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SpiritMod.Gores
{
	public class AsteroidDebrisSmall : ModGore
	{
		public override void OnSpawn(Gore gore, IEntitySource source)
		{
			gore.numFrames = 3;
			gore.frame = (byte)Main.rand.Next(gore.numFrames);
			gore.timeLeft = Gore.goreTime;
			ChildSafety.SafeGore[gore.type] = true;
		}

		public override bool Update(Gore gore)
		{
			gore.timeLeft = Math.Min(gore.timeLeft, Gore.goreTime);
			gore.timeLeft--;

			gore.position += gore.velocity;
			gore.velocity *= 0.99f;
			gore.rotation += gore.velocity.Length() / 40;

			//Allow the player to "bump" asteroid debris
			//if (Main.LocalPlayer.Hitbox.Intersects(new Rectangle((int)gore.position.X, (int)gore.position.Y, (int)gore.Width, (int)gore.Height)))
			//	gore.velocity += Main.LocalPlayer.velocity * .5f;

			if (gore.timeLeft <= 255)
				if (++gore.alpha >= 255)
					gore.active = false;
			return false;
		}
	}
}