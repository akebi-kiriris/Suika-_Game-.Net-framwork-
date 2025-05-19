Imports System.IO

Public Class Form1
    Dim balls As New List(Of Ball)()
    Const GRAVITY As Double = 1.0 ' 重力加速度
    Const BOUNCE_FACTOR As Double = 0.15 ' 彈跳因子，控制彈跳時的速度減緩
    Const VELOCITY_THRESHOLD As Double = 0.8 ' 速度閾值，當速度小於此值時視為靜止
    Private windowSnapshot As Bitmap
    Private updating As Boolean = False


    ' 圖片路徑陣列
    Private ballImages As String() = {"C:\Users\USER\Desktop\resourse\1.png", "C:\Users\USER\Desktop\resourse\2.png", "C:\Users\USER\Desktop\resourse\3.png", "C:\Users\USER\Desktop\resourse\4.png", "C:\Users\USER\Desktop\resourse\5.png", "C:\Users\USER\Desktop\resourse\6.png", "C:\Users\USER\Desktop\resourse\7.png", "C:\Users\USER\Desktop\resourse\8.png"}

    Private Class Ball
        Public Property X As Double
        Public Property Y As Double
        Public Property VelocityX As Double
        Public Property VelocityY As Double
        Public Property Radius As Double
        Public Property Mass As Double
        Public Property ImagePath As String ' 新增圖片路徑屬性

        Public Sub New(x As Double, y As Double, velocityX As Double, velocityY As Double, radius As Double, mass As Double, imagePath As String)
            Me.X = x
            Me.Y = y
            Me.VelocityX = velocityX
            Me.VelocityY = velocityY
            Me.Radius = radius
            Me.Mass = mass
            Me.ImagePath = imagePath
        End Sub

        ' 新增方法計算球的半徑，根據圖片編號調整大小
        Public Function CalculateRadius() As Double
            Dim imageNumber As Integer = GetImageNumber(ImagePath)
            ' 根據你的規則調整球的大小，這裡示例是每加1，直徑增加2
            Return 20 + (imageNumber - 1) * 2
        End Function
    End Class

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Size = New Size(500, 700) ' 設定視窗大小為 500*700
        ' 啟動Timer
        Timer1.Start()
    End Sub

    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        ' 畫出球
        For Each ball In balls
            ' 調整球的大小
            Dim ballRadius As Double = ball.CalculateRadius()

            ' 從選定的路徑載入圖片
            Using ballImage As Image = Image.FromFile(ball.ImagePath)
                ' 根據球的半徑調整位置
                e.Graphics.DrawImage(ballImage, CInt(ball.X - ballRadius), CInt(ball.Y - ballRadius), CInt(ballRadius * 2), CInt(ballRadius * 2))
            End Using
        Next
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim anyBallMoving As Boolean = False

        ' 複製一份球的列表，避免在迭代中修改
        Dim ballsCopy As List(Of Ball) = balls.ToList()
        If Not updating Then
            ' 更新球的位置
            For Each ball In ballsCopy
            ' 判斷是否碰到底部
            If ball.Y + ball.Radius >= Me.ClientSize.Height Then
                ball.Y = Me.ClientSize.Height - ball.Radius
                ball.VelocityY = -ball.VelocityY * BOUNCE_FACTOR ' 考慮彈跳效果

                ' 檢查彈跳後的速度是否太小，若是則將速度設為零
                If Math.Abs(ball.VelocityY) < VELOCITY_THRESHOLD Then
                    ball.VelocityY = 0
                End If
            End If

            ' 判斷是否碰到左右邊界
            If ball.X - ball.Radius <= 0 OrElse ball.X + ball.Radius >= Me.ClientSize.Width Then
                ball.VelocityX = -ball.VelocityX * BOUNCE_FACTOR ' 考慮彈跳效果

                ' 檢查彈跳後的速度是否太小，若是則將速度設為零
                If Math.Abs(ball.VelocityX) < VELOCITY_THRESHOLD Then
                    ball.VelocityX = 0
                End If
            End If

            ' 更新位置
            ball.X += ball.VelocityX
            ball.Y += ball.VelocityY

            ' 更新速度，模擬重力影響
            ball.VelocityY += GRAVITY

            Dim friction As Double = 0.3 ' 這是一個可以調整的摩擦力係數
            ball.VelocityX -= Math.Sign(ball.VelocityX) * friction
            ball.VelocityY -= Math.Sign(ball.VelocityY) * friction

            ' 檢查是否有碰撞
            For Each otherBall In ballsCopy
                If ball IsNot otherBall Then
                    Dim dx As Double = otherBall.X - ball.X
                    Dim dy As Double = otherBall.Y - ball.Y
                    Dim distance As Double = Math.Sqrt(dx * dx + dy * dy)
                    Dim minDistance As Double = ball.Radius + otherBall.Radius

                    ' 如果碰撞，進行反彈處理
                    If distance < minDistance Then
                        ' 調整球的位置，防止球重疊
                        Dim overlap As Double = minDistance - distance
                        Dim adjustX As Double = overlap * (dx / distance) / 2
                        Dim adjustY As Double = overlap * (dy / distance) / 2

                        ball.X -= adjustX
                        ball.Y -= adjustY
                        otherBall.X += adjustX
                        otherBall.Y += adjustY

                        ' 計算碰撞後的速度
                        CalculateCollision(ball, otherBall)
                    End If
                End If
            Next

            If Math.Abs(ball.VelocityX) >= VELOCITY_THRESHOLD OrElse Math.Abs(ball.VelocityY) >= VELOCITY_THRESHOLD Then
                anyBallMoving = True
            End If
        Next

            If anyBallMoving Then
                Me.Invalidate() ' 強制重繪
            End If
            updating = False
        End If

    End Sub


    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        ' 隨機選擇一個圖片路徑
        Dim randomImagePath As String = ballImages(New Random().Next(0, 4))

        ' 在固定的高度50新增一顆球，X值隨滑鼠的位置改變
        balls.Add(New Ball(e.X, 50, 0, 0, 40, 1.0, randomImagePath)) ' 初始半徑為20，質量為1.0
    End Sub


    ' 計算碰撞後的新速度
    Private Sub CalculateCollision(ball1 As Ball, ball2 As Ball)
        Dim dx As Double = ball2.X - ball1.X
        Dim dy As Double = ball2.Y - ball1.Y
        Dim distance As Double = Math.Sqrt(dx * dx + dy * dy)

        ' 計算碰撞時的法向量（單位向量）
        Dim normalX As Double = dx / distance
        Dim normalY As Double = dy / distance

        ' 計算碰撞時的切線向量（單位向量）
        Dim tangentX As Double = -normalY
        Dim tangentY As Double = normalX

        ' 將速度向量投影到法向量和切線向量上
        Dim v1n As Double = normalX * ball1.VelocityX + normalY * ball1.VelocityY
        Dim v1t As Double = tangentX * ball1.VelocityX + tangentY * ball1.VelocityY
        Dim v2n As Double = normalX * ball2.VelocityX + normalY * ball2.VelocityY
        Dim v2t As Double = tangentX * ball2.VelocityX + tangentY * ball2.VelocityY

        ' 計算新的法向量速度（考慮質量）
        Dim newV1n As Double = ((ball1.Mass - ball2.Mass) * v1n + 2 * ball2.Mass * v2n) / (ball1.Mass + ball2.Mass)
        Dim newV2n As Double = ((ball2.Mass - ball1.Mass) * v2n + 2 * ball1.Mass * v1n) / (ball1.Mass + ball2.Mass)

        ' 將法向量速度和切線向量速度重新組合成新的速度向量
        ball1.VelocityX = newV1n * normalX + v1t * tangentX
        ball1.VelocityY = newV1n * normalY + v1t * tangentY
        ball2.VelocityX = newV2n * normalX + v2t * tangentX
        ball2.VelocityY = newV2n * normalY + v2t * tangentY

        ' 判斷是否需要合成
        If ShouldMerge(ball1, ball2) Then
            ' 合成兩個球，將球2的圖片路徑設為球1的下一張圖片路徑
            ball1.ImagePath = GetNextImagePath(ball1.ImagePath)
            ' 移除球2
            balls.Remove(ball2)
        End If
    End Sub

    ' 判斷是否需要合成
    Private Function ShouldMerge(ball1 As Ball, ball2 As Ball) As Boolean
        ' 在這裡添加合成的條件，比如說碰撞的兩個球的圖片編號相鄰且不是圖片8
        Dim imageNumber1 As Integer = GetImageNumber(ball1.ImagePath)
        Dim imageNumber2 As Integer = GetImageNumber(ball2.ImagePath)

        ' 根據你的需求定義合成的條件
        Return Math.Abs(imageNumber1 - imageNumber2) = 0 AndAlso imageNumber1 <> 8
    End Function

    ' 取得圖片編號
    Private Function GetImageNumber(imagePath As String) As Integer
        ' 假設圖片路徑的格式是 "C:\...\resourse\X.png"，其中 X 為圖片編號
        Dim fileName As String = Path.GetFileNameWithoutExtension(imagePath)
        Dim imageNumber As Integer
        If Integer.TryParse(fileName, imageNumber) Then
            Return imageNumber
        Else
            Return 0 ' 如果無法解析編號，返回預設值
        End If
    End Function

    ' 取得下一張圖片路徑
    Private Function GetNextImagePath(currentImagePath As String) As String
        ' 根據你的命名規則取得下一張圖片的路徑
        Dim imageNumber As Integer = GetImageNumber(currentImagePath)
        Dim nextImageNumber As Integer = imageNumber + 1
        Dim directoryPath As String = Path.GetDirectoryName(currentImagePath)
        Return Path.Combine(directoryPath, $"{nextImageNumber}.png")
    End Function

    Public Sub New()
        Me.DoubleBuffered = True
        InitializeComponent()
    End Sub
End Class
