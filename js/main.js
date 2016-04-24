$.fn.inView = function (topPad,objSize) {
    var win = $(window);
    obj = $(this);
    var scrollPosition = win.scrollTop() + topPad + (objSize / 2);
    var visibleArea = win.scrollTop() + topPad + (objSize / 2) + win.height() - topPad;
    var objEndPos = (obj.offset().top + obj.outerHeight());
    return (visibleArea >= objEndPos && scrollPosition <= objEndPos ? true : false)
};

class SherHome {
    constructor() {
        this.map = null;
        this.markers = [];
        this.openMarker = null;

    }

    initializeMap() {
        var mapOptions = {
            zoom: 14,
            center: { lat: 45.400993, lng: -71.882429 }
        };
        this.map = new google.maps.Map(document.getElementById('mapcontainer'),
              mapOptions);
    }

    addMarker(lat,lng,title,iconUrl,info) {
        var marker = new google.maps.Marker({
            position: { lat: lat, lng: lng },
            map: this.map,
            title: title,
            icon: iconUrl,
            clickable: true
        });

        if (info != null && info.length > 0)
        {
            marker.info = new google.maps.InfoWindow({
                content: info
            });
            var map = this.map;
            var self = this;
            google.maps.event.addListener(marker, 'click', function () {
                if (self.openMarker != null)
                {
                    self.openMarker.info.close();
                }
                marker.info.open(map, marker);
                self.openMarker = marker;
            });
        }
        this.markers.push(marker);
    }

    clearMarkers() {
        for(var i = 0; i < this.markers.length; i++)
        {
            this.markers[i].setMap(null);
        }
        while (this.markers.length > 0) {
            this.markers.pop();
        }
    }

    setMapCenter(lat, lng)
    {
        this.map.setCenter(new google.maps.LatLng(lat,lng));
    }

    find()
    {
        var self = this;
        $('.resultcontent').each(function (i, obj) {
            var $elem = $(obj);

            if ($elem.inView(300,250) && $elem != this.focusedElement)
            {
                var url = obj.className.replace("resultcontent ", "");
                self.focusOnResult(url,$elem);
                return false;
            }
            else
            {
                return true;
            }
        });
    
    }

    setResults(container,data)
    {
        $(container).parent().scrollTop(0);
       $(container).empty();
       this.focusedElement = null;
       this.openMarker = null;
       this.resultSet = new Object();

       for (var i = 0; i < data.results.length; i++)
       {
           var dataSet = data.results[i].markers;
           var result = data.results[i];
           var ad = new Object();
           ad["title"] = result["ad"]["title"];
           ad["description"] = result["ad"]["desc"];
           ad["address"] = result["ad"]["address"];
           ad["latitude"] = result["ad"]["latitude"];
           ad["longitude"] = result["ad"]["longitude"];
           ad["price"] = result["ad"]["price"];
           ad["image"] = result["ad"]["image"];
           ad["url"] = result["ad"]["url"];
           ad["rank"] = result["ad"]["rank"];
           if (!(ad.url in this.resultSet) && ad.image != null && ad.image.length > 0)
           {
               this.addResult(container, ad.title, ad.address, ad.price, ad.image, ad.description, ad.rank, ad.url);
               this.resultSet[ad.url] = { ad: ad, dataSet: dataSet };
           }
       }
       this.find();
    }

    focusOnResult(url,elem)
    {
        this.openMarker = null;
        var data = this.resultSet[url];
        if(data != null)
        {
            var adData = data.dataSet;
            var ad = data.ad;
            this.clearMarkers();
            this.setMapCenter(parseFloat(ad.latitude), parseFloat(ad.longitude));

            this.addMarker(parseFloat(ad.latitude), parseFloat(ad.longitude), ad.address, this.getIcon("home"), ad.address);
            for(var i = 0; i < adData.length; i++)
            {
                var info = adData[i].title;
        
                
                this.addMarker(parseFloat(adData[i].latitude), parseFloat(adData[i].longitude), adData[i].title, this.getIcon(adData[i].type), info);
            }

            if (this.focusedElement != null)
            {
                $(this.focusedElement).removeClass('selectedresult');
            }

            this.focusedElement = elem;
            $(this.focusedElement).addClass('selectedresult');
        }
    }

    addResult(container, title, address, price, image, description, relevance, url) {
        var resultContent = $("<div>", { class: "resultcontent " + url });
        var resultImage = $("<img>", { class: "resultimage" });
        $(resultImage).attr('src', image);

        var resultTextWrapper = $("<div>", { class: "resulttextwrapper" });
        var resultTextContainerTitle = $("<div>", { class: "resulttextcontainer" });
        var resultTextContainerAddress = $("<div>", { class: "resulttextcontainer" });
        var resultTextContainerDescription = $("<div>", { class: "resulttextcontainer" });
        var resultTextContainerLink = $("<div>", { class: "resulttextcontainer" });

        var resultTextTitle = $("<p>", { class: "resulttitle" });
        var resultTextAddress = $("<p>", { class: "resultsubtitle" });
        var resultTextDescription = $("<p>", { class: "resultdescription" });
        var resultTextLink = $("<a>", { class: "resultlink" });

        var resultPrice = $("<p>", { class: "resultprice" });
        var resultRating = $("<div>", { class: "resultrating" });

        var relevancePercent = parseInt(relevance * 100, 10).toString() + '%';
        var relevancePercentTiny = parseInt((relevance - 0.05) * 100, 10).toString() + '%';

        $(resultTextTitle).html(title);
        $(resultTextAddress).html(address);
        $(resultTextDescription).html(description);
        $(resultTextLink).attr('href', url);
        $(resultTextLink).attr('target', '_blank');
        $(resultTextLink).html('Learn More');
        $(resultRating).html(relevancePercent);
        $(resultPrice).html(price);
        $(resultRating).height(relevancePercentTiny);

        $(resultTextContainerTitle).append(resultTextTitle);
        $(resultTextContainerAddress).append(resultTextAddress);
        $(resultTextContainerDescription).append(resultTextDescription);
        $(resultTextContainerLink).append(resultTextLink);

        $(resultTextWrapper).append(resultTextContainerTitle);
        $(resultTextWrapper).append(resultTextContainerAddress);
        $(resultTextWrapper).append(resultTextContainerDescription);
        $(resultTextWrapper).append(resultTextContainerLink);

        $(resultContent).append(resultImage);
        $(resultContent).append(resultTextWrapper);
        $(resultContent).append(resultPrice);
        $(resultContent).append(resultRating);

        $(container).append(resultContent);
    }

    createOptions() {
        var self = this;
        $('.sidebarslider').each(function (i, obj) {
            $(obj).slider({
                step: 1, min: -5, max: 5, value: 0, change: function (event, ui) {
                    self.update();
                }
            });
        });
    }

    getIcon(type)
    {
        if (type == "bus")
            return "Icons/bus.png";
        else if (type == "zap")
            return "Icons/wifi.png";
        else if (type == "hospitals")
            return "Icons/hospital.png";
        else if (type == "schools")
            return "Icons/school.png";
        else if (type == "ecocenters")
            return "Icons/eco.png";
        else if (type == "events")
            return "Icons/event.png";
        else if (type == "home")
            return "Icons/house.png";
        else if (type == "camps")
            return "Icons/camp.png";
        else if (type == "graffiti")
            return "Icons/graffiti.png";
        else if (type == "stores")
            return "Icons/shopping.png";
        else if (type == "attractions")
            return "Icons/attraction.png";
        else if (type == "restaurants")
            return "Icons/restaurant.png";
        else if (type == "parks")
            return "Icons/park.png";
        else if (type == "golf")
            return "Icons/golf.png";

        return "";
    }

    update()
    {
        var values = new Object();
        $('.sidebarslider').each(function (i, obj) {
            values[$(obj).attr('id')] = $(obj).slider("option", "value") / 5;
        });
        var self = this;
        var valuesArr = [];
        valuesArr.push(values);
        $('#resultcontainer').block({ message: $('#throbber') });
        $.ajax({
            type: "POST",
            url: "Query.aspx",
            data: JSON.stringify(valuesArr),
            success: function (msg) {
                var data = msg;
                self.setResults("#resultwrap", data);
            }
            ,
            error: function (msg,x,thrown) {
            },
            complete: function (data) {
                $('#resultcontainer').unblock();
            },

            timeout:15000
        });
    }
}

var sherHome = new SherHome();


function initMap()
{
    sherHome.initializeMap();
    sherHome.clearMarkers();

    $('#results').scroll(function () {
        sherHome.find();
    });

    sherHome.createOptions();

    $.blockUI.defaults.css = {
        padding: 0,
        margin: 0,
        width: '30%',
        top: '40%',
        left: '35%',
        textAlign: 'center',
        cursor: 'wait'
    };
}

function initApp()
{
    // Call the initialize function after the page has finished loading
    google.maps.event.addDomListener(window, 'load', initMap);
}

