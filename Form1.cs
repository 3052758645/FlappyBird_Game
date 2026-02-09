using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlappyBird_Game {
    public partial class Form1 : Form {
        //------------------------------------------------
        int birdY = 250;
        int birdVelocity = 0;
        int gravity = 1;
        int jumpStrength = -15;

        List<Rectangle> pipes = new List<Rectangle>();
        int pipesWidth = 60;
        int pipeGap = 180;
        int pipeSpeed = 5;
        int pipeSpawnTimer = 0;
        int pipeSpawnInterval = 100;

        bool gameStarted = false;
        bool gameOver = false;
        int score = 0;
        HashSet<int> scoredPipes = new HashSet<int>();
        int highScore = 0;

        Brush birdBrush = Brushes.Yellow;
        Brush pipeBrush = Brushes.Green;
        Brush backgroundBrush = new SolidBrush(Color.FromArgb(135, 206, 235));
        Brush groundBrush = new SolidBrush(Color.FromArgb(222, 184, 135));

        const int GroundHeight = 50;
        //------------------------游戏资源(设定)----------------------------


        public Form1() {
            InitializeComponent();
            ResetGame();
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        private void ResetGame() {
            birdY = this.Height / 2;
            birdVelocity = 0;
            pipes.Clear();
            pipeSpawnTimer = 0;
            score = 0;
            scoredPipes.Clear();
            gameOver = false;
            gameStarted = false;

            gameTimer.Stop();
        }

        void StartGame() {
            if (!gameStarted) {
                gameStarted = true;
                gameTimer.Start();
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e) {
            if (!gameStarted || gameOver) return;

            birdVelocity += gravity;
            birdY += birdVelocity;

            pipeSpawnTimer++;
            if (pipeSpawnTimer >= pipeSpawnInterval) {
                SpawnPipe();
                pipeSpawnTimer = 0;
            }

            MovePipes();
            CheckScore();
            CheckCollisions();
            this.Invalidate();
        }

        private void CheckScore() {
            Rectangle birdRect = new Rectangle(100, birdY, 40, 30);
            for (int i = 0; i < pipes.Count; i += 2) {
                Rectangle upperPipe = pipes[i];
                int pipeId = upperPipe.Height + upperPipe.X;
                if (birdRect.X > upperPipe.X + upperPipe.Width && !scoredPipes.Contains(pipeId)) {
                    score++;
                    scoredPipes.Add(pipeId);
                }
            }
        }

        private void CheckCollisions() {
            Rectangle birdRect = new Rectangle(100, birdY, 40, 30);

            if (birdY <= 0 || birdY + 30 >= this.Height - GroundHeight) {
                GameOver();
                return;
            }

            foreach (Rectangle pipe in pipes) {
                if (birdRect.IntersectsWith(pipe)) {
                    GameOver();
                    return;
                }
            }
        }

        private void GameOver() {
            gameOver = true;
            gameTimer.Stop();
            if (score > highScore) {
                highScore = score;
            }
        }

        private void MovePipes() {
            for (int i = pipes.Count - 1; i >= 0; i--) {
                Rectangle pipe = pipes[i];
                pipe.X -= pipeSpeed;
                pipes[i] = pipe;
                if (pipe.X + pipe.Width < 0) {
                    int pipeId = pipe.Height + (this.Width);
                    scoredPipes.Remove(pipeId);
                    pipes.RemoveAt(i);
                }
            }
        }

        private void SpawnPipe() {
            Random random = new Random();
            int pipeHeight = random.Next(100, this.Height - pipeGap - GroundHeight - 100);

            pipes.Add(new Rectangle(this.Width, 0, pipesWidth, pipeHeight));
            pipes.Add(new Rectangle(this.Width, pipeHeight + pipeGap, pipesWidth, this.Height - pipeHeight - pipeGap - GroundHeight));
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Space) {
                HandleInput();
            } else if (e.KeyCode == Keys.Enter && gameOver) {
                ResetGame();
                StartGame();
            } else if (e.KeyCode == Keys.Escape) {
                Application.Exit();
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            HandleInput();
        }

        private void HandleInput() {
            if (!gameStarted) {
                StartGame();
            }

            if (!gameOver) {
                birdVelocity = jumpStrength;
            }
        }
        /// <summary>
        /// 涂色
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.FillRectangle(backgroundBrush, 0, 0, this.Width, this.Height - GroundHeight);
            g.FillRectangle(groundBrush, 0, this.Height - GroundHeight, this.Width, GroundHeight);

            foreach (Rectangle pipe in pipes) {
                g.FillRectangle(pipeBrush, pipe);

                using (Pen pipeEdgePen = new Pen(Color.DarkGreen, 3)) {
                    g.DrawRectangle(pipeEdgePen, pipe);
                }

                Rectangle pipeCap = new Rectangle(pipe.X - 5, pipe.Y, pipe.Width + 10, 20);
                if (pipe.Y == 0) {
                    pipeCap.Y = pipe.Height - 20;
                }

                g.FillRectangle(Brushes.DarkGreen, pipeCap);
                g.DrawRectangle(Pens.Black, pipeCap);
            }

            g.FillEllipse(birdBrush, 100, birdY, 40, 30);
            g.FillEllipse(Brushes.White, 125, birdY + 8, 10, 10);
            g.FillEllipse(Brushes.Black, 127, birdY + 10, 6, 6);

            Point[] beak = new Point[]
            {
                new Point(140, birdY + 15),
                new Point(150, birdY + 12),
                new Point(150, birdY + 18)
            };
            g.FillPolygon(Brushes.Orange, beak);

            // 绘制分数
            using (Font scoreFont = new Font("Arial", 36, FontStyle.Bold)) {
                string scoreText = $"Score: {score}";
                g.DrawString(scoreText, scoreFont, Brushes.Black, 15, 15);
                g.DrawString(scoreText, scoreFont, Brushes.White, 10, 10);
            }

            if (!gameStarted) {
                Font font2 = new Font("微软雅黑", 45, FontStyle.Bold);
                using (Font startFont = new Font("Arial", 27, FontStyle.Bold)) {
                    string startText = "Flappy bird";
                    string text2 = "By-Lx理想";
                    string text3 = "按空格或者点击开始游戏";
                    SizeF textSize = g.MeasureString(startText, startFont);
                    g.DrawString(startText, font2, Brushes.White,
                        (float)((this.Width - textSize.Width) / 2.35), (float)(this.Height / 3.5));
                    g.DrawString(text2, startFont, Brushes.Blue,
                        (float)((this.Width - textSize.Width) / 1.9999), (float)(this.Height / 2.39999999));
                    g.DrawString(text3, startFont, Brushes.White,
                        (float)((this.Width - textSize.Width) / 2.578), (float)(this.Height / 2));
                }
            }

            if (gameOver) {
                using (Brush overlayBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0))) {
                    g.FillRectangle(overlayBrush, 0, 0, this.Width, this.Height);
                }

                using (Font gameOverFont = new Font("Arial", 48, FontStyle.Bold)) {
                    string gameOverText = "游戏结束!";
                    SizeF textSize = g.MeasureString(gameOverText, gameOverFont);
                    g.DrawString(gameOverText, gameOverFont, Brushes.Red,
                        (this.Width - textSize.Width) / 2, this.Height / 3);
                }

                using (Font scoreFont = new Font("Arial", 24, FontStyle.Bold)) {
                    string scoreText = $"当前得分: {score}";
                    SizeF scoreSize = g.MeasureString(scoreText, scoreFont);
                    g.DrawString(scoreText, scoreFont, Brushes.Yellow,
                        (this.Width - scoreSize.Width) / 2, this.Height / 2);

                    string highScoreText = $"历史最高分: {highScore}";
                    SizeF highScoreSize = g.MeasureString(highScoreText, scoreFont);
                    g.DrawString(highScoreText, scoreFont, Brushes.LimeGreen,
                        (this.Width - highScoreSize.Width) / 2, this.Height / 2 + 40);

                    string restartText = "按回车键重新开始";
                    SizeF restartSize = g.MeasureString(restartText, scoreFont);
                    g.DrawString(restartText, scoreFont, Brushes.White,
                        (this.Width - restartSize.Width) / 2, this.Height / 2 + 80);
                }
            }
        }

    }
}