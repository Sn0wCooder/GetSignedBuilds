Public Class Form1
    Dim da As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\"
    Public signedios As String()
    Public signeduris As String()
    Dim WC As New System.Net.WebClient

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CheckForInternetConnection() = True Then
            If System.IO.File.Exists(da & "fws.json") Then
                System.IO.File.Delete(da & "fws.json")
            End If
            WC.DownloadFile("https://api.ipsw.me/v2.1/firmwares.json", da & "fws.json")
        Else
            MsgBox("ERROR: this application require an internet connection to work.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub
    Public Function CheckForInternetConnection() As Boolean
        Try
            Using client = New System.Net.WebClient()
                Using stream = client.OpenRead("http://www.google.com")
                    Return True
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    Public Function GetSignedSHSHs(ByVal info As String)
        If Not System.IO.File.Exists(da & "fws.json") Then
            WC.DownloadFile("https://api.ipsw.me/v2.1/firmwares.json", da & "fws.json")
        End If
        ' I know there are better ways to read a Json file in vb :/

        Dim Json() As String = IO.File.ReadAllLines(da & "fws.json")

        Dim Jsonlines As Integer = Json.Length
        Dim position As Integer = 0
        Dim buildoffset As Integer = 0
        Dim versoffset As Integer = 0
        Dim urisoffset As Integer = 0
        Dim rightdevice As Boolean = False

        Dim signedbuilds As String() = {}
        Dim signedios1 As String() = {}
        Dim signeduris1 As String() = {}
        Dim signedcount As Integer = -1

        Dim open = 0
        Dim closed = 0

        Do While position <> Jsonlines

            If Json(position).Contains(info) Then
                rightdevice = True
            End If

            If Json(position).Contains("{") And rightdevice = True Then
                open = open + 1
            End If

            If Json(position).Contains("}") And rightdevice = True Then
                closed = closed + 1
            End If

            If open = closed And open <> 0 Then
                rightdevice = False
            End If

            If Json(position).Contains("""" + "version" + """" + ": ") And rightdevice = True Then
                versoffset = position
            End If

            If Json(position).Contains("""" + "buildid" + """" + ": ") And rightdevice = True Then
                buildoffset = position
            End If

            If Json(position).Contains("""" + "url" + """" + ": ") And rightdevice = True Then
                urisoffset = position
            End If

            If Json(position).Contains("true") And rightdevice = True Then
                signedcount = signedcount + 1

                Dim b = Json(versoffset).Replace("""" + "version" + """" + ": " + """", "").Replace("""" + ",", "").Trim()
                Dim a = Json(buildoffset).Replace("""" + "buildid" + """" + ": " + """", "").Replace("""" + ",", "").Trim()
                Dim c = Json(urisoffset).Replace("""" + "url" + """" + ": " + """", "").Replace("""" + ",", "").Trim()

                ReDim Preserve signedbuilds(signedcount)
                signedbuilds(signedcount) = a

                ReDim Preserve signedios1(signedcount)
                signedios1(signedcount) = b

                ReDim Preserve signeduris1(signedcount)
                signeduris1(signedcount) = c

            End If

            position = position + 1
        Loop


        signedios = signedios1
        signeduris = signeduris1
        Return signedbuilds
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim SignedBuilds As String() = GetSignedSHSHs(TextBox1.Text)
        If SignedBuilds.Count = 0 Then
            MsgBox("ERROR: device invalid!", MsgBoxStyle.Critical, "Error")
        Else
            ListBox1.Items.Clear()
            GroupBox1.Text = "Signed builds for: " & TextBox1.Text
            For Each builds In SignedBuilds
                ListBox1.Items.Add(builds)
            Next
        End If
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Process.Start("https://github.com/Sn0wCooder/GetSignedBuilds")
    End Sub
End Class
