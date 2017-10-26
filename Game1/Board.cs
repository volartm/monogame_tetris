using System;
using System.IO;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tetris
{
    public class Board : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //assets
        Texture2D background;

        Texture2D textures;
        readonly Rectangle blockRectangle = new Rectangle();
        SpriteFont gameFont;

        int cellSize = 24;
        private static int width = 10, height = 20;
        int[,] boardMatrix = new int[width, height];

        Figure fig, previwFigure;
        private float movement, speed = 0.05f;
        private float score = 0, RecordScore = 0;
        Queue<Figure> figureQueue = new Queue<Figure>();

        // Input
        KeyboardState oldKeyboardState = Keyboard.GetState();

        bool pause = false;

        private float timeTick = 0;
        private bool canReadKeaState = true;
        private bool canPlaceFigure = true;

        public Board()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferHeight = (height )* cellSize+12;
            graphics.PreferredBackBufferWidth = width * cellSize + 12;
            this.IsFixedTimeStep = true;
            this.graphics.SynchronizeWithVerticalRetrace = true;
            this.TargetElapsedTime = new System.TimeSpan(0, 0, 0, 0, 33); // 33ms = 30fps

            //assets
            Content.RootDirectory = "Content";
            blockRectangle = new Rectangle(0, 0, 24, 24);

            //boardMatrix
            boardMatrix.Initialize();

            figureQueue.Enqueue(new Figure());
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Try to open file if it exists, otherwise create it
            using (FileStream fileStream = File.Open("record.dat", FileMode.OpenOrCreate))
            {
                fileStream.Close();
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("background");

            textures = Content.Load<Texture2D>("block");
            // Load game font
            //gameFont = Content.Load<SpriteFont> ("font");
            gameFont = Content.Load<SpriteFont>("Arial");

            // Load game record
            using (StreamReader streamReader = File.OpenText("record.dat"))
            {
                int record = 0;
                if ((record = Convert.ToInt32(streamReader.ReadLine())) != 0)
                    RecordScore = record;
            }

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            // Check pause
            bool pauseKey = (oldKeyboardState.IsKeyDown(Keys.P) && (keyboardState.IsKeyUp(Keys.P)));

            oldKeyboardState = keyboardState;

            if (pauseKey)
                pause = !pause;

            if (!pause )//&& IsActive)
            {
                if (figureQueue.Count == 1)
                {
                    fig = figureQueue.Dequeue();
                    Debug.WriteLine(tryPlaceFigure(fig));

                    previwFigure = new Figure();
                    if (!tryPlaceFigure(fig))
                    {
                        canPlaceFigure = false;
                    }
                    else
                    {
                        canPlaceFigure = true;
                    }
                }

                if (timeTick > 0.3)
                {
                    timeTick = 0;
                    canReadKeaState = true;
                }
                else
                {
                    timeTick += 0.1f;
                }

                if (!canPlaceFigure)
                {
                    GameOver();
                }
                else
                {
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                        ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        Exit();
                    // If left key is pressed
                    if ((keyboardState.IsKeyDown(Keys.Left)) && canReadKeaState)
                    {
                        if (canMoveLeft(fig))
                            fig.X -= 1;
                        canReadKeaState = false;
                    }
                    // If right key is pressed
                    if ((keyboardState.IsKeyDown(Keys.Right)) && canReadKeaState)
                        if (canMoveRight(fig))
                        {
                            fig.X += 1;
                            canReadKeaState = false;
                        }
                    // If down key is pressed
                    if (keyboardState.IsKeyDown(Keys.Down))
                        if (canMoveDown(fig))
                            fig.Y += 1;
                    // Rotate figure
                    if ((keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space)) && canReadKeaState)
                    {
                        rotate(fig);
                        canReadKeaState = false;
                    }

                    // Moving figure
                    if (Movement >= 1)
                    {
                        Movement = 0;

                        if (!canMoveDown(fig))
                        {
                            figureQueue.Enqueue(previwFigure);

                            for (int i = 0; i < fig.Matrix.GetLength(0); i++)
                            {
                                for (int k = 0; k < fig.Matrix.GetLength(1); k++)
                                {
                                    if (fig.Matrix[i, k] == 1)
                                    {
                                        boardMatrix[fig.X + i, fig.Y + k] = 1;
                                    }
                                }
                            }

                            checkFullLine();
                        }
                        else
                        {
                            fig.Y += 1;
                        }
                    }
                    else
                    {
                        Movement += speed;
                    }
                    base.Update(gameTime);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // draw the background
            spriteBatch.Draw(background, new Vector2(5, 5), color: Color.White);
            //draw falling figure
            for (int i = 0; i < fig.Matrix.GetLength(0); i++)
            {
                for (int k = 0; k < fig.Matrix.GetLength(1); k++)
                {
                    if (fig.Matrix[i, k] == 1)
                    {
                        spriteBatch.Draw(textures, new Vector2((fig.X + i) * cellSize + 5, (fig.Y + k) * cellSize + 5),
                            blockRectangle, Color.Green);
                    }
                }
            }

            //draw fallen figures
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    if (boardMatrix[i, k] == 1)
                    {
                        spriteBatch.Draw(textures, new Vector2(i * cellSize + 5, k * cellSize + 5), blockRectangle,
                            Color.Green);
                    }
                }
            }

            //draw preview figure
            if (previwFigure != null)
            {
                for (int i = 0; i < previwFigure.Matrix.GetLength(0); i++)
                {
                    for (int k = 0; k < previwFigure.Matrix.GetLength(1); k++)
                    {
                        if (previwFigure.Matrix[i, k] == 1)
                        {
                            spriteBatch.Draw(textures, new Vector2((i+15) * cellSize*0.5f, (k+3) * cellSize*0.5f), null, Color.Red, 0f,
                                Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
                        }
                    }
                }
            }

            spriteBatch.DrawString(gameFont, "Score:\n" + score , new Vector2(cellSize + 8, cellSize + 9), Color.Orange);
            spriteBatch.DrawString(gameFont, "Record:\n" + RecordScore , new Vector2(cellSize + 8, 3 * cellSize + 9), Color.Orange);

            base.Draw(gameTime);
            spriteBatch.End();
        }

        public float Movement
        {
            set { movement = value; }
            get { return movement; }
        }

        private bool canMoveDown(Figure figure)
        {
            for (int k = fig.Matrix.GetLength(1) - 1; k + 1 > 0; k--)
            {
                for (int i = 0; i < fig.Matrix.GetLength(0); i++)
                {
                    if ((figure.Y + k) + 2 > height)
                    {
                        return false;
                    }

                    if (figure.Matrix[i, k] == 1 && boardMatrix[figure.X + i, figure.Y + k + 1] == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool canMoveLeft(Figure figure)
        {
            for (int k = fig.Matrix.GetLength(1) - 1; k + 1 > 0; k--)
            {
                for (int i = 0; i < fig.Matrix.GetLength(0); i++)
                {
                    if (figure.X - 1 < 0)
                    {
                        return false;
                    }

                    if (boardMatrix[figure.X + i - 1, figure.Y + k] == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool canMoveRight(Figure figure)
        {
            for (int k = fig.Matrix.GetLength(1) - 1; k + 1 > 0; k--)
            {
                for (int i = fig.Matrix.GetLength(0) - 1; i + 1 > 0; i--)
                {
                    if (figure.X + i + 2 > width)
                    {
                        return false;
                    }

                    if (boardMatrix[figure.X + i + 1, figure.Y + k] == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void checkFullLine()
        {
            bool isFullLine = true;

            for (int k = height - 1; k + 1 > 0; k--)
            {
                isFullLine = true;
                for (int i = 0; i < width; i++)
                {
                    if (boardMatrix[i, k] != 1)
                    {
                        isFullLine = false;
//                        break;
                    }
                }
                if (isFullLine)
                {
                    shiftDownCells(k);
                    speed += 0.005f;
                    score += speed * 100;
                    break;
                }
            }
        }

        private void shiftDownCells(int line)
        {
            for (int k = line; k > 0; k--)
            {
                for (int i = 0; i < width; i++)
                {
                    boardMatrix[i, k] = boardMatrix[i, k - 1];
                }
            }

            checkFullLine();
        }

        private bool tryPlaceFigure(Figure figure)
        {

            for (int i = 0; i < fig.Matrix.GetLength(0); i++)
            {
                for (int k = 0; k < fig.Matrix.GetLength(1); k++)
                {
                    if (fig.Matrix[i, k] ==1 && boardMatrix[fig.X + i, fig.Y + k] == fig.Matrix[i, k] )
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void rotate(Figure figure)
        {
            int h = figure.Matrix.GetLength(1), w = figure.Matrix.GetLength(0);
            int[,] tmp= new int[h,w];
            bool canRotate = true;
            for (int i = 0; i < w;  i++)
            {
                for (int k = h - 1; k >=0; k--)
                {
                    tmp[ h - 1 - k , i] = figure.Matrix[i, k];
                    if (figure.X + h - k > width || figure.X + h - 1 - k < 0 || figure.Y + i +1 > height
                        || tmp[h - 1 - k, i] == 1 && boardMatrix[figure.X + h - 1 - k, figure.Y + i] == tmp[h - 1 - k, i])
                        canRotate = false;
                }
            }
            if (canRotate)
            {
                figure.Matrix = tmp;
                if (figure.Matrix.GetLength(0) == 4)
                {
                    figure.X -= 1;
                    figure.Y += 1;
                }
                if (figure.Matrix.GetLength(1) == 4)
                {
                    figure.X += 1;
                    figure.Y -= 1;
                }
            }

        }

        private void GameOver()
        {
            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < width; i++)
                {
                    boardMatrix[i, k] = 0;
                }
            }

            if (score > RecordScore)
            {
                RecordScore = score;

                pause = true;

                using (StreamWriter writer = File.CreateText("record.dat"))
                {
                    writer.WriteLine(RecordScore);
                }

                pause = false;
            }

            speed = 0.05f;
            timeTick = 0;
            Movement = 0;
            canPlaceFigure = true;
            score = 0;
        }

    }
}