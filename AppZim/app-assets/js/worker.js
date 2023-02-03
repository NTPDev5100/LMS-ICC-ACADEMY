function workerRunner() {
    self.onmessage = (event) => {
        var blob = event.data.file;
        var bytesPerChunk = 52428800;
        var size = blob.size;
        var start = 0;
        var end = bytesPerChunk;
        var completed = 0;
        var count = size % bytesPerChunk == 0 ? size / bytesPerChunk : Math.floor(size / bytesPerChunk) + 1;
        var counter = 0;
        

        var uploadCompleted = function () {
            var formData = new FormData();
            formData.append('fileName', event.data.file.name);
            formData.append('domain', event.data.domain);
            formData.append('scheduleid', event.data.hdscheduleid);
            formData.append('complete', true);
            //$.ajax({
            //    async: false,
            //    type: 'POST',
            //    url: '/Admin/TeacherCourse/UploadComplete',
            //    data: formData,
            //    contentType: false,
            //    processData: false
            //});

            var xhr2 = new XMLHttpRequest();
            xhr2.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    self.postMessage({ result: true });
                }
            };
            xhr2.open("POST", event.data.domain + "/Admin/TeacherCourse/UploadComplete", true);
            xhr2.send(formData);
        }
        var multiUpload = function (count, counter, blob, completed, start, end, bytesPerChunk) {
            counter = counter + 1;
            if (counter <= count) {
                var chunk = blob.slice(start, end);
                var xhr = new XMLHttpRequest();
                xhr.onload = function () {
                    start = end;
                    end = start + bytesPerChunk;
                    if (count == counter) {
                        uploadCompleted();
                    } else {
                        multiUpload(count, counter, blob, completed, start, end, bytesPerChunk);
                    }
                }
                xhr.open("POST", event.data.domain + "/Admin/TeacherCourse/MultiUpload?id=" + counter.toString() + "&fileName=" + event.data.file.name, true);
                xhr.send(chunk);
                
                //start = end;
                //end = start + bytesPerChunk;
                //var formdataVideo = new FormData();
                //formdataVideo.append('file', chunk);
                //formdataVideo.append('id', counter.toString());
                //formdataVideo.append('fileName', event.data.file.name.toString());
                //$.ajax({
                //    async: false,
                //    type: 'POST',
                //    url: '/Admin/TeacherCourse/MultiUpload',
                //    data: formdataVideo,
                //    contentType: false,
                //    processData: false
                //});           
            }
            //if (count == counter) {
            //    uploadCompleted();
            //} else {
            //    multiUpload(count, counter, blob, completed, start, end, bytesPerChunk);
            //}
        }

        


        multiUpload(count, counter, blob, completed, start, end, bytesPerChunk);
    }
}