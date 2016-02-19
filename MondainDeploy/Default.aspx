<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs"Inherits="MondainDeploy.Default" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!-- Latest compiled and minified CSS -->
    <link rel="stylesheet" href="http://code.jquery.com/ui/1.11.4/themes/smoothness/jquery-ui.css"/>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css"/>
 
<!-- Latest compiled and minified JavaScript -->
    <script type="text/javascript" src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js"></script>
    
    <%--JQuery--%>
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.3/jquery.min.js"></script>
    <script type="text/javascript" src="//code.jquery.com/ui/1.11.4/jquery-ui.js"></script>
    <title>Mondain Words</title>
    <style type="text/css">
        p.small {
    line-height: 110%;
                }

        p.big {
    line-height: 140%;
        }
    </style>
    
    <%--Project specific--%>
    <link href="Style_Sheets/MondainDeployBSTheme.css" rel="stylesheet" type="text/css" />
    <link href="Style_Sheets/ProgressBarStyle.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="Scripts/ProgressBarScript.js"></script>
    <link href="Style_Sheets/TooltipStyle.css" rel="stylesheet" />
    <script type="text/javascript" src="Scripts/TooltipScript.js"></script>
</head>
<body>
    <div class="container">
        <div class="page-header">
        <h1>Mondain Words</h1>
        <p class="lead">Word quizzing program, created by Quinn James</p>
      </div>
        <div id="progressbar">
           
        </div>
        <div id="separator"><br/></div>
        <br />

        
        <form id="form1" runat="server">
            <div class="row">
            <div class="col-sm-4">
              <div class="panel panel-info">
                <div class="panel-heading">
                  <h3 class="panel-title">Actions</h3>
                </div>
                <div class="panel-body">
                    <asp:Panel id="Panel2" runat="server">
                    <p>Quiz Questions: <asp:TextBox ID="TBQuizLength" runat="server" Width="40"></asp:TextBox></p>
                    <p><asp:CheckBox ID="BlankBingoCheck" runat="server"/> Blank Bingo mode</p>
                    <p><asp:CheckBox ID="LexSymbolCheck" runat="server"/> Enable lexicon symbols</p>
                    <p class="big">
                    Length: Min <asp:DropDownList ID="MinDD" runat="server" Width="60px" Type="Integer" Display="Dynamic"></asp:DropDownList>&nbsp;&nbsp;&nbsp;
                     Max <asp:DropDownList ID="MaxDD" runat="server" Width="60px"></asp:DropDownList></p>
                    <p>Min Probability<sup title="Calculated per word. Currently ignored by Blank Bingo mode.">[info]</sup>: Min <asp:TextBox ID="MinProb" runat="server" Width="50px"></asp:TextBox> 
                    Max <asp:TextBox ID="MaxProb" runat="server" Width="50px"></asp:TextBox></p>
                        
                    <p class="big">
                        <asp:Button ID="StartQuiz" runat="server" CssClass="btn btn-primary" onClick="cmdStartQuiz_Click" Text="Start Quiz" />
                    </p>

                        </asp:Panel>
                </div>
              </div>
                
                
            <!-- Validators -->
                <asp:RangeValidator id="rangeQuizLength" runat="server" ControlToValidate="TBQuizLength"
                                    MaximumValue="100" MinimumValue="1" Type="Integer"
                                    errormessage="Quiz length must be 1-100." Display="none"/>
                <asp:CompareValidator runat="server" id="cmpProb" ControlToValidate="MinProb" 
                                    controltocompare="MaxProb" operator="LessThanEqual" type="Integer"
                                    errormessage="Max probability cannot be greater than min probability." Display="none" />
                <asp:CompareValidator runat="server" id="cmpLength" ControlToValidate="MinDD"
                                    controltocompare="MaxDD" operator="LessThanEqual" type="Integer"
                                    errormessage="Max length cannot be greater than minimum length." Display="none" />
                <asp:RegularExpressionValidator runat="server" id="regexpAnswer" ControlToValidate="TBQuizAnswer"
                                    ErrorMessage="Guesses should contain only letters."
                                    ValidationExpression="^[a-zA-Z]+$" Display="None" />
                
                <asp:CustomValidator id="customQuizLength" runat="server"
                                    ErrorMessage="Quiz length can't be shorter than the specified probability range."
                                    ValidateEmptyText="False"
                                    ControlToValidate="TBQuizLength"
                                    OnServerValidate="customQuizLength_ServerValidate"
                                    Display="None"/>
                

              <div class="panel panel-info">
                <div class="panel-heading">
                  <h3 class="panel-title">Quiz Status</h3>
                </div>
                <div class="panel-body">
                           <div class="alert alert-info alert-dismissable">
                            <asp:ValidationSummary ID="validationSummary" runat="server"
                            ShowModelStateErrors="true" />
                       </div>
                    <asp:Label ID="CurrentStatus" runat="server" Text=""></asp:Label><br/>

                </div>  


              </div>
            </div><!-- /.col-sm-4 -->
            <div class="col-sm-4">
              <div class="panel panel-primary">
                <div class="panel-heading">
                    

                  <h3 class="panel-title">Quiz</h3>
                </div>
                <div class="panel-body">


                    <asp:Label ID="LabelCurrentQuestion" runat="server" Text="#0: none" CssClass="h4"/><br/>
                                Total solutions: <asp:Label ID="LabelTotalSolutions" runat="server" Text=""></asp:Label><br/>
                    Correct answers:<br/> <asp:Label ID="CurrentQuestionHistoryLabel" runat="server" Text=""></asp:Label>        

                                     <asp:Panel ID="Panel1" runat="server" defaultbutton="BTAnswer">
                                         
                                        

                                         <asp:TextBox ID="TBQuizAnswer" runat="server" Text="" Width="250"></asp:TextBox><br/>
                                <asp:Button ID="BTAnswer" runat="server" OnClick="SubmitAnswerButton_Click" Text="Answer" CssClass="btn btn-primary" />&nbsp;&nbsp;&nbsp;&nbsp;
                                      <asp:Button ID="BTMarkMissed" runat="server" OnClick="MarkMissedButton_Click" Text="Mark Missed" CssClass="btn" /><br/>       

                                     </asp:Panel>
        
                </div>
                  
              </div>
              <div class="panel panel-info">
                <div class="panel-heading">
                  <h3 class="panel-title">Statistics</h3>
                </div>
                <div class="panel-body">
                        <div class="col-md-6">
                            <table class="table table-bordered">
                                <thead>
                                  <tr>
                                    <th></th>
                                    <th>Correct</th>
                                    <th>Percent</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <tr>
                                    <td>Per Alphagram</td>
                                     <td><asp:Label ID="Label_StatsCorrectAlphagramFraction" runat="server" Text="0/0"/></td>
                                     <td><asp:Label ID="Label_StatsCorrectAlphagramPercent" runat="server" Text="0%"/></td>
                                  </tr>
                                  <tr>
                                    <td>Per Word</td> 
                                     <td><asp:Label ID="Label_StatsCorrectWordFraction" runat="server" Text="0/0"/></td>
                                     <td><asp:Label ID="Label_StatsCorrectWordPercent" runat="server" Text="0%"/></td>
                                  </tr>
                                </tbody>
                              </table>
                            </div>                
                        </div>
                  </div>
            </div><!-- /.col-sm-4 -->
            <div class="col-sm-4">
              <div class="panel panel-info">
                <div class="panel-heading">
                  <h3 class="panel-title">Answer History</h3>
                </div>
                <div class="panel-body">
                                        <asp:Panel runat="server" Height="300px" ScrollBars="Vertical">
                  <asp:Label ID="AnswerHistory" runat="server" Text=""></asp:Label>
                        </asp:Panel>

                </div>
              </div>
            </div><!-- /.col-sm-4 -->
        </div>
    </form>
</div>
</body>
</html>

