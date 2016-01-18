
$(document).ready(function () {
    var questionNumber = document.getElementById("LabelCurrentQuestion").innerHTML.substring(1).split(":")[0];
    var quizLength = document.getElementById("TBQuizLength").value;
    var quizPercent = questionNumber / quizLength * 100;

    $("#progressbar").progressbar({
        value: quizPercent
    });
});

