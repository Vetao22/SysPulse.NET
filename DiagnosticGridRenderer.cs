using System;

namespace SysPulse.NET
{
    /// <summary>
    /// Specialized grid renderer leveraging TerminalGrid conventions to draw widgets and progress bars.
    /// </summary>
    internal sealed class DiagnosticGridRenderer
    {
        public void DrawHeader(char[,] buffer, string title)
        {
            int width = buffer.GetLength(1);
            for (int i = 0; i < title.Length && i < width; i++)
            {
                buffer[0, i] = title[i];
            }
        }

        public void DrawText(char[,] buffer, int startX, int startY, string text)
        {
            int height = buffer.GetLength(0);
            int width = buffer.GetLength(1);

            if (startY < 0 || startY >= height) return;

            for (int i = 0; i < text.Length; i++)
            {
                int x = startX + i;
                if (x >= 0 && x < width)
                {
                    buffer[startY, x] = text[i];
                }
            }
        }

        public void DrawProgressBar(char[,] buffer, int startX, int startY, int width, string label, double percentage)
        {
            DrawText(buffer, startX, startY, $"{label}: {percentage:F1}%");
            
            int barY = startY + 1;
            int height = buffer.GetLength(0);
            int screenWidth = buffer.GetLength(1);

            if (barY < 0 || barY >= height) return;

            buffer[barY, startX] = '[';
            buffer[barY, startX + width + 1] = ']';

            int innerWidth = width;
            int filledChars = (int)Math.Round((percentage / 100.0) * innerWidth);
            filledChars = Math.Clamp(filledChars, 0, innerWidth);

            for (int i = 0; i < innerWidth; i++)
            {
                int x = startX + 1 + i;
                if (x >= 0 && x < screenWidth)
                {
                    buffer[barY, x] = i < filledChars ? '█' : '░';
                }
            }
        }

        public void DrawPanelBox(char[,] buffer, int startX, int startY, int width, int height, string title)
        {
            int screenHeight = buffer.GetLength(0);
            int screenWidth = buffer.GetLength(1);

            // Draw Top & Bottom borders
            for (int x = 0; x < width; x++)
            {
                int currX = startX + x;
                if (currX >= 0 && currX < screenWidth)
                {
                    if (startY >= 0 && startY < screenHeight) buffer[startY, currX] = '─';
                    int bottomY = startY + height - 1;
                    if (bottomY >= 0 && bottomY < screenHeight) buffer[bottomY, currX] = '─';
                }
            }

            // Draw Side borders
            for (int y = 0; y < height; y++)
            {
                int currY = startY + y;
                if (currY >= 0 && currY < screenHeight)
                {
                    if (startX >= 0 && startX < screenWidth) buffer[currY, startX] = '│';
                    int rightX = startX + width - 1;
                    if (rightX >= 0 && rightX < screenWidth) buffer[currY, rightX] = '│';
                }
            }

            // Corners
            if (startY >= 0 && startY < screenHeight && startX >= 0 && startX < screenWidth) buffer[startY, startX] = '┌';
            if (startY >= 0 && startY < screenHeight && startX + width - 1 >= 0 && startX + width - 1 < screenWidth) buffer[startY, startX + width - 1] = '┐';
            
            int bY = startY + height - 1;
            if (bY >= 0 && bY < screenHeight && startX >= 0 && startX < screenWidth) buffer[bY, startX] = '└';
            if (bY >= 0 && bY < screenHeight && startX + width - 1 >= 0 && startX + width - 1 < screenWidth) buffer[bY, startX + width - 1] = '┘';

            // Title
            if (!string.IsNullOrEmpty(title))
            {
                int titleX = startX + 2;
                DrawText(buffer, titleX, startY, $" {title} ");
            }
        }
    }
}