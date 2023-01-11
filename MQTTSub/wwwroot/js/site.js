$(document).ready(function () {

    console.log("loaded");

    var topics = [];
    var macAddrs = [];
    var brokerIP;
    var intervals = [];
    var previousMessage = [];
    var recentMessage = [];
    var macFilterArr = [];
    var tableIndex = 1;
    var pause = false;
    var macFilter = "";
    var isMacFilter = false;
    var buffer = [];

    function filterBuffer() {

    }

    function displayMessages(message, topic) {
        let date = new Date();
        let createdAt = date.getTime();
        let table = $('#messageTable').DataTable();
        if (isMacFilter) {
            for (let i = 0; i < macFilterArr.length; i++) {
                if (message.includes(macFilterArr[i])) {
                    table.row.add([tableIndex, message, topic, createdAt]).draw(false);
                    break;
                }
            }
        } else {
            table.row.add([tableIndex, message, topic, createdAt]).draw(false);
        }

        tableIndex++;
    }

    function filterDuplicatedMessage(message, topic) {
        console.log('Checked topic = ' + topic);
        let checkIndex = topics.findIndex(function (topicName) {
            return topicName == topic;
        });
        if (recentMessage[checkIndex] != message) {
            console.log('New message is ' + message);
            previousMessage[checkIndex] = recentMessage[checkIndex];
            recentMessage[checkIndex] = message;
            if (!pause) {
                displayMessages(message, topic);
            }
        } else {
            console.log('Message exists');
        }

    }

    function clearIntervals() {
        for (let i = 0; i < intervals.length; i++) {
            clearInterval(intervals[i]);
            console.log('cleared interval id ' + intervals[i]);
        }
        intervals.length = 0;
    }

    function setCallback(brokerIP, topic) {
        $.get("https://localhost:44382/" + brokerIP + "/" + encodeURIComponent(topic) + "/messages", function cb(res, status) {
            //console.log(status);
            //console.log(res);
            if ((res !== undefined) && (res.length > 0)) {
                filterDuplicatedMessage(res, topic);
            }
        });
    }

    function initTask() {
        topics = $("#topicSelector").select2('data');
        brokerIP = $("#brokerSelector").val();

        console.log(topics);
        console.log(brokerIP);

        for (let i = 0; i < topics.length; i++) {
            recentMessage.push(" ");
            previousMessage.push(" ");
        }
        //console.log(recentMessage.length);
        //console.log(previousMessage.length);

        for (let i = 0; i < topics.length; i++) {
            let encoded_url = "https://localhost:44382/subscribe/" + brokerIP + "/" + encodeURIComponent(topics[i].id);
            console.log("encoded url = " + encoded_url);
            $.get(encoded_url, function cb(res, status) {
                console.log(status);
            });

            //Set interval function to get message data from topics and broker choosen 
            let id = setInterval(setCallback, 1000, brokerIP, topics[i].id);
            console.log("interval id = " + id);
            intervals.push(id);
            console.log(topics[i].id);
        }
    }

    //Init topic selector
    $("#topicSelector").select2();

    $("#b_connect").click(function () {
        //console.log("Butt Caption changed");
        if ($("#b_connect").text() == "Connect") {
            macFilter = $("#macFilter").val();
            if (macFilter.length > 0) {
                isMacFilter = true;
                let temp = macFilter.includes(" ");
                while (temp) {
                    macFilter = macFilter.replace(" ", "");
                    temp = macFilter.includes(" ");
                }
                console.log(macFilter);
                macFilterArr = macFilter.split(",");
                console.log(macFilterArr);
            }
            $("#b_connect").html("Disconnect");
            //Send request to server that start subscribing to broker IP and topics choosen
            initTask();
        } else {
            isMacFilter = false;
            $("#b_connect").html("Connect");
            clearIntervals();
            $.get("https://localhost:44382/disconnect/" + brokerIP);
            $("#b_pause").html("Pause");
            console.log('Disconnected');
        }
        pause = false;
    });

    $("#b_clear").click(function () {
        let table = $("#messageTable").DataTable().clear().draw();
    });

    $("#b_pause").click(function () {
        if ($("#b_pause").text() == "Pause") {
            $("#b_pause").html("Resume");
            pause = true;
        } else {
            $("#b_pause").html("Pause");
            pause = false;
        }
    });

    $("#messageTable").DataTable({
        scrollX: 200,
        scroller: true
    });

});