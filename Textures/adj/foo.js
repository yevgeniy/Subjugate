var fs=require('fs');

var files = fs.readdirSync('./')

files=files.map(v=>v.split('.')[0]);

var str=`
	var files = new List<string>{
		"${files.join('", "')}"
	}
`;

console.log(str);