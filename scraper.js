var request = require("request");
var cheerio = require("cheerio");
var jar = request.jar();
var fs = require('fs');

var parseHTML = function(html) {
    var ad = {};
    var $ = cheerio.load(html);

    ad.title = $("span[itemprop=name]").text();
    ad.image = $("img[itemprop=image]").attr("src");
    ad.latitude = $('meta[property="og:latitude"]').attr('content');
    ad.longitude = $('meta[property="og:longitude"]').attr('content');
    

    $("#MapLink").remove();
    $(".divider").remove();
    
    $("table.ad-attributes tr").each(function(i,tr) {
        var field = $(tr).find("th").text().trim();
        var value = $(tr).find("td").text().trim();
                
        ad[field] = value;
    });

    ad.desc = $("span[itemprop=description]").text().trim();
    
    return ad;
}

var scrape = function(url, callback, index, urls, adUrls, rootUrl,filename,adObjects) {
    if (url === undefined)
	    return callback(new Error("URL must not be undefined"), null, index, callback,urls,adUrls,rootUrl,filename,adObjects);

    request({
  uri: url,
  method: "GET",
  timeout: 3000,
  jar: jar
}, function(err, res, body) {
        if (err) 
    	{
    		callback(err, null, index, callback,urls,adUrls,rootUrl,filename,adObjects);
    		return;	
    	}
        var ad = parseHTML(body);
        ad.url = url;
        callback(null, ad, index, callback,urls,adUrls,rootUrl,filename,adObjects);
    });
}

var adCallback = function(error, ad, index, callback,urls,adUrls,rootUrl,filename,adObjects)
{
	if(error == null)
	{
		adObjects.push(ad);
		console.log("Finished reading ad " + adUrls[index]);
	}
	else
	{
		console.log(error);
		scrape(adUrls[index],callback,index,urls,adUrls,rootUrl,filename,adObjects);
		return;
	}

	if(index + 1 < adUrls.length)
	{
		scrape(adUrls[index+1],callback,index+1,urls,adUrls,rootUrl,filename,adObjects);
	}
	else
	{
		fs.writeFile(filename, JSON.stringify(adObjects), function(err) {
    if(err) {
        return console.log(err);
    }

    console.log("The file " + filename + " " + "was saved.");
});
	}
}

var urlCrawler = function(url, pageNo, urlIndex, urls,adUrls,rootUrl,filename,adObjects) {
console.log("Reading " + url +  "...");
	request({
  uri: url,
  timeout: 3000,
  method: "GET",
  jar: jar
}, function(err, res, body) {
	console.log("Finished reading page " + pageNo + ".");
			        if (err) 
		        	{
		        		urlCrawler(url,pageNo,urlIndex,urls,adUrls,rootUrl,filename,adObjects);
		        		return;
		        	}
			        var $ = cheerio.load(body);
			        var title = $('title').text();
			        if(pageNo == 1 || title.includes('Page ' + pageNo))
			        {
			        	$titles = $("a[class='title']");

			        	for(var i = 0; i < $titles.length; i++)
			        	{
			        		var adUrl = $($titles[i]).attr('href');
			        		if(!adUrl.includes('src=topAdSearch'))
			        		{
					        	if(adUrls.indexOf(rootUrl + adUrl) == -1)
					        	{
					        		adUrls.push(rootUrl + adUrl);			        		
					        	}
			        		}
			        	}
			        	var nextUrl = urls[urlIndex].replace("$",pageNo+1);
			        	urlCrawler(nextUrl,pageNo+1,urlIndex,urls,adUrls,rootUrl,filename,adObjects);
			        }
			        else
			        {
			        	urlIndex++;
			        	if(urlIndex < urls.length)
			        	{
			        		console.log('Starting url index ' + urlIndex);	
			        		var nextUrl = urls[urlIndex].replace("$",1);
			        		urlCrawler(nextUrl,1,urlIndex,urls,adUrls,rootUrl,filename,adObjects);
			        	}
			        	else
			        	{
			        		//we are done, fetch the ads
			        		console.log('Starting ad scraping');	
			        		scrape(adUrls[0],adCallback,0,urls,adUrls,rootUrl,filename,adObjects);
			        	}
			        }
			    });
}

var main = function()
{
	var urls = ['http://www.kijiji.ca/b-appartement-condo/sherbrooke-qc/page-$/c37l1700156r5.0?ad=offering',
		'http://www.kijiji.ca/b-chambres-a-louer-colocataire/sherbrooke-qc/page-$/c36l1700156r5.0?ad=offering',
		'http://www.kijiji.ca/b-maison-a-louer/sherbrooke-qc/page-$/c43l1700156r5.0?ad=offering',
		'http://www.kijiji.ca/b-maison-a-vendre/sherbrooke-qc/page-$/c35l1700156r5.0?ad=offering'];

	var adUrls = [];
	var rootUrl = 'http://www.kijiji.ca/';
	var filename = 'kijiji.json';
	var adObjects = [];

	jar.setCookie("siteLocale=en_CA",rootUrl, {}, function(){});
	urlCrawler(urls[0].replace('$',1),1,0,urls,adUrls,rootUrl,filename,adObjects);
}

main();


