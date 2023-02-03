!function() {
    
  /* authors : TCT base on Paul Navasard https://codepen.io/peanav/details/ulkof */
  var today = moment();

  function Calendar(selector, events) {
    this.el = document.querySelector(selector);
    this.events = events;
    this.current = moment().date(1);
    this.events.forEach(function(ev) {
     ev.date = moment(ev.date);
    }); 
    this.draw();
    
    var current = document.querySelector('.today');
    if(current) {
      var self = this;
      window.setTimeout(function() {
          /*self.openDay(current);*/
      }, 500);
    }
    
  }

  Calendar.prototype.draw = function() {
    //Create Header
    this.drawHeader();

    //Draw Month
    this.drawMonth();

    this.drawLegend();  
      
  }

  Calendar.prototype.drawHeader = function() {
    var self = this;
    if(!this.header) {
      //Create the header elements
      this.header = createElement('div', 'header');
      this.header.className = 'header';

      this.title = createElement('h1');

      var right = createElement('div', 'right');
      right.addEventListener('click', function() { self.nextMonth(); });

      var left = createElement('div', 'left');
      left.addEventListener('click', function() { self.prevMonth(); });

      //Append the Elements
      this.header.appendChild(this.title); 
      this.header.appendChild(right);
      this.header.appendChild(left);
      this.el.appendChild(this.header);
    }

    this.title.innerHTML = this.current.format('MMMM YYYY');
  }

  Calendar.prototype.drawMonth = function() {
    var self = this;
    
    if(this.month) {
      this.oldMonth = this.month;
      this.oldMonth.className = 'month out animation ' + (self.next ? 'next' : 'prev');
      this.oldMonth.addEventListener('webkitAnimationEnd', function() {
        self.oldMonth.parentNode.removeChild(self.oldMonth);
        self.month = createElement('div', 'month');
        self.backFill();
        self.currentMonth();
        self.fowardFill();
        self.el.appendChild(self.month);
        window.setTimeout(function() {
          self.month.className = 'month in animation ' + (self.next ? 'next' : 'prev');
        }, 16);
      });
    } else {
        this.month = createElement('div', 'month');
        this.el.appendChild(this.month);
        this.backFill();
        this.currentMonth();
        this.fowardFill();
        this.month.className = 'month new';
    }
  }

  Calendar.prototype.backFill = function() {
    var clone = this.current.clone();
    var dayOfWeek = clone.day();

    if(!dayOfWeek) { return; }

    clone.subtract('days', dayOfWeek+1);

    for(var i = dayOfWeek; i > 0 ; i--) {
      this.drawDay(clone.add('days', 1));
    }
  }

  Calendar.prototype.fowardFill = function() {
    var clone = this.current.clone().add('months', 1).subtract('days', 1);
    var dayOfWeek = clone.day();

    if(dayOfWeek === 6) { return; }

    for(var i = dayOfWeek; i < 6 ; i++) {
      this.drawDay(clone.add('days', 1));
    }
  }

  Calendar.prototype.currentMonth = function() {
    var clone = this.current.clone();

    while(clone.month() === this.current.month()) {
      this.drawDay(clone);
      clone.add('days', 1);
    }
  }

  Calendar.prototype.getWeek = function(day) {
    if(!this.week || day.day() === 0) {
      this.week = createElement('div', 'week');
      this.month.appendChild(this.week);
    }
  }

  Calendar.prototype.drawDay = function(day) {
    var self = this;
    this.getWeek(day);

    //Outer Day
    var outer = createElement('div', this.getDayClass(day));
    outer.addEventListener('click', function() {
      self.openDay(this);
    });

    //Day Name
    var name = createElement('div', 'day-name', day.format('ddd'));

    //Day Number
    var number = createElement('div', 'day-number', day.format('DD'));


    //Events
    var events = createElement('div', 'day-events');
    this.drawEvents(day, events);
    outer.setAttribute('data-date-string', day.format('YYYY-MM-DD').toString());
    outer.appendChild(name);
    outer.appendChild(number);
    outer.appendChild(events);
    this.week.appendChild(outer);
  }
  Calendar.prototype.pushEvent = function(event){
    
      event.date = moment(event.date);
      event.id = event.id;
      event.limit = parseInt(event.limit);
      this.events.push(event);
      
      var dateString = event.date.format('YYYY-MM-DD');
      var dateEl = this.month.querySelector('[data-date-string="'+dateString+'"]');
   
        if(dateEl){
            var outerEv =  dateEl.getElementsByClassName('day-events')[0];
            outerEv.innerHTML = '';
            this.drawEvents(event.date, outerEv);

            this.reRenderEvents(moment(event.date).format('YYYY-MM-DD'));

        }
       this.drawLegend();  
  }
  Calendar.prototype.ejectEvent = function(event){
      event.date = moment(event.date);
      
	  
      this.events = this.events.filter(function(el) {
           return el.id !== event.id;
      });
      
	  
      var dateString = event.date.format('YYYY-MM-DD');
      var dateEl = this.month.querySelector('[data-date-string="'+dateString+'"]');
      if(dateEl){
        var outerEv =  dateEl.getElementsByClassName('day-events')[0];
          outerEv.innerHTML = '';
          this.drawEvents(event.date, outerEv);
          
         
          
         /* Check is current date */
        this.reRenderEvents(moment(event.date).format('YYYY-MM-DD'));
          //push blank ev
          
            event.eventName = 'Blank Event';
            event.calendar = 'Blank';
            event.color = 'blank';
            this.pushEvent(event);

      }
      this.drawLegend();  
  }
  Calendar.prototype.reRenderEvents = function(day){
  
      day = moment(day).format('YYYY-MM-DD');
      var dateEl = this.month.querySelector('[data-date-string="'+day+'"]');
      var details = dateEl.parentElement.querySelector('.details[data-for="'+day+'"]');
      
        if(details){
            var limit = details.getAttribute('data-limit');
            var dayNumber = ''+dateEl.querySelectorAll('.day-number')[0].innerText || ''+dateEl.querySelectorAll('.day-number')[0].textContent;
            var day = moment(dateEl.getAttribute('data-date-string').toString());

            var todaysEvents = this.events.reduce(function(memo, ev) {
              if(ev.date.isSame(day, 'day')) {
                memo.push(ev);
              }
              return memo;
            }, []);

            var currentWrapper = details.querySelector('.events');
            currentWrapper.innerHTML = '';
            if(!todaysEvents.length) {
                if(limit > 0){
                    
                } else {
                    var div = createElement('div', 'event empty');
                    var span = createElement('span', '', 'No Events');

                    div.appendChild(span);
                    currentWrapper.appendChild(div);
                }
              
            } else {
                todaysEvents.forEach(function(ev) {
                    if(ev.calendar.toLowerCase() == 'blank' || ev.color.toLowerCase() == 'blank'){
                        
                    }else{
                        
                        var div = createElement('div', 'event');
                        var square = createElement('div', 'event-category ' + ev.color);
                        var span = createElement('span', '', ev.eventName);
                        var ip = createElement('input', '', ev.calendar);
                        ip.setAttribute('type', 'hidden');
                        ip.setAttribute('name', 'calendar');
                        var ip2 = createElement('input', '', ev.id);
                        ip2.setAttribute('type', 'hidden');
                        ip2.setAttribute('name', 'id');
                            
                        
                        div.appendChild(square);
                        div.appendChild(span);
                        div.appendChild(ip);
                        div.appendChild(ip2);
                        if(ev.hasOwnProperty('tiet')){
                            if(ev.tiet.length > 0){
                                var ul = createElement('div', 'tiethoc');
                                ev.tiet.forEach(function(item){
                                    var ipT = createElement('input', '', item.id+','+item.name);
                                    ipT.setAttribute('name', 'tT');
                                    ipT.setAttribute('type', 'hidden');
                                    ul.appendChild(ipT);

                                });
                                div.appendChild(ul);
                            }

                        }
                            
                        currentWrapper.appendChild(div);
                    }
                      
                });
                todaysEvents = todaysEvents.filter(function(ev){
                    return  ev.calendar.toLowerCase() !== 'blank';
                });
                this.renderEventsCB(details, todaysEvents);
            }
            
            
        }
  }
  Calendar.prototype.drawEvents = function(day, element) {
    if(day.month() === this.current.month()) {
        
      var todaysEvents = this.events.reduce(function(memo, ev) {
        if(ev.date.isSame(day, 'day')) {
          memo.push(ev);
        }
        return memo;
      }, []);

//        console.log(evHolder);
      todaysEvents.forEach(function(ev) {
           var lim = ev.hasOwnProperty('limit') ? ev.limit : 0;
            element.setAttribute('data-limit', lim);
           
            var evCount = element.childElementCount;
          
            while(lim > evCount){
                var evBlank = createElement('span', 'blank');
                
                element.appendChild(evBlank);
                evCount++; 
            }
          
            var evSpan = createElement('span', ev.color);
            evSpan.title = ev.calendar || ev.color;
            if(element.querySelector('span.blank')){
                element.removeChild(element.querySelector('span.blank'));
            }    
            element.appendChild(evSpan);
      });
    }
  }

  Calendar.prototype.getDayClass = function(day) {
    classes = ['day'];
    if(day.month() !== this.current.month()) {
      classes.push('other');
    } else if (today.isSame(day, 'day')) {
      classes.push('today');
    }
    return classes.join(' ');
  }

  Calendar.prototype.openDay = function(el) {
    var details, arrow;
    var dayNumber = +el.querySelectorAll('.day-number')[0].innerText || +el.querySelectorAll('.day-number')[0].textContent;
      
    var day = moment(el.getAttribute('data-date-string').toString());
    var limit = el.querySelector('.day-events').getAttribute('data-limit') || 0;
    var currentOpened = document.querySelector('.details');

    /* Check to see if there is an open detais box on the current row */
    if(currentOpened && currentOpened.parentNode === el.parentNode) {
      details = currentOpened;
      arrow = document.querySelector('.arrow');
    } else {
      /* Close the open events on differnt week row */
      /* currentOpened && currentOpened.parentNode.removeChild(currentOpened); */
      if(currentOpened) {
        currentOpened.addEventListener('webkitAnimationEnd', function() {
          currentOpened.parentNode.removeChild(currentOpened);
        });
        currentOpened.addEventListener('oanimationend', function() {
          currentOpened.parentNode.removeChild(currentOpened);
        });
        currentOpened.addEventListener('msAnimationEnd', function() {
          currentOpened.parentNode.removeChild(currentOpened);
        });
        currentOpened.addEventListener('animationend', function() {
          currentOpened.parentNode.removeChild(currentOpened);
        });
        currentOpened.className = 'details out';
      }

      /* Create the Details Container */
      details = createElement('div', 'details in');

      /* Create the arrow */
      var arrow = createElement('div', 'arrow');

      /* Create the event wrapper */

      details.appendChild(arrow);
      el.parentNode.appendChild(details);
      
    }
    

    var todaysEvents = this.events.reduce(function(memo, ev) {
      if(ev.date.isSame(day, 'day')) {
        memo.push(ev);
      }
      return memo;
    }, []);
     
    details.setAttribute('data-for', el.getAttribute('data-date-string').toString());
    details.setAttribute('data-limit', limit);
    this.renderEvents(todaysEvents, details);

    arrow.style.left = el.offsetLeft - el.parentNode.offsetLeft + el.offsetWidth/2 + 'px';
      
  }

  Calendar.prototype.renderEvents = function(events, ele) {
       var self = this;
    //Remove any events in the current details element
    var currentWrapper = ele.querySelector('.events');
    var wrapper = createElement('div', 'events in' + (currentWrapper ? ' new' : ''));

    events.forEach(function(ev) {
        if(ev.calendar.toLowerCase() == 'blank' || ev.color.toLowerCase() == 'blank'){
            
        } else {
                var div = createElement('div', 'event');
                var square = createElement('div', 'event-category ' + ev.color);
                var span = createElement('span', '', ev.eventName);
                var ip = createElement('input', '', ev.calendar);
                ip.setAttribute('type', 'hidden');
                ip.setAttribute('name', 'calendar');
                var ip2 = createElement('input', '', ev.id);
                ip2.setAttribute('type', 'hidden');
                ip2.setAttribute('name', 'id');

                div.appendChild(square);
                div.appendChild(span);
                div.appendChild(ip);
                div.appendChild(ip2);
                if(ev.hasOwnProperty('tiet')){
                    if(ev.tiet.length > 0){
                        var ul = createElement('div', 'tiethoc');
                        ev.tiet.forEach(function(item){
                            var ipT = createElement('input', '', item.id+','+item.name);
                            ipT.setAttribute('name', 'tT');
                            ipT.setAttribute('type', 'hidden');
                            ul.appendChild(ipT);

                        });
                        div.appendChild(ul);
                    }

                }
              wrapper.appendChild(div);
        }
          
    });

    if(!events.length) {
      var div = createElement('div', 'event empty');
      var span = createElement('span', '', 'No Events');

      div.appendChild(span);
      wrapper.appendChild(div);
    }
      //filter blank events 
events = events.filter(function(ev){
    return  ev.calendar.toLowerCase() !== 'blank';
});
    if(currentWrapper) {
      currentWrapper.className = 'events out';
      currentWrapper.addEventListener('webkitAnimationEnd', function() {
        currentWrapper.parentNode.removeChild(currentWrapper);
        ele.appendChild(wrapper);
        self.renderEventsCB(ele , events);
      });
      currentWrapper.addEventListener('oanimationend', function() {
        currentWrapper.parentNode.removeChild(currentWrapper);
        ele.appendChild(wrapper);
          self.renderEventsCB(ele, events);
      });
      currentWrapper.addEventListener('msAnimationEnd', function() {
        currentWrapper.parentNode.removeChild(currentWrapper);
        ele.appendChild(wrapper);
          self.renderEventsCB(ele, events);
      });
      currentWrapper.addEventListener('animationend', function() {
        currentWrapper.parentNode.removeChild(currentWrapper);
        ele.appendChild(wrapper);
          
        self.renderEventsCB(ele, events);
      });
    } else {
      ele.appendChild(wrapper);
        self.renderEventsCB(ele , events);
    }
  }

  Calendar.prototype.drawLegend = function() {
      var legend;
      
      var getlegend = document.querySelector('#calendar .legend');
      if( getlegend != null){
          legend = getlegend;
          legend.innerHTML = '';
      } else {
          legend = createElement('div', 'legend');
      }
    
    var calendars = this.events.map(function(e) {
      return e.calendar + '|' + e.color;
    }).reduce(function(memo, e) {
      if(memo.indexOf(e) === -1) {
        memo.push(e);
      }
      return memo;
    }, []).forEach(function(e) {
      var parts = e.split('|');
      var entry = createElement('span', 'entry ' +  parts[1], parts[0]);
      legend.appendChild(entry);
    });
  
    this.el.appendChild(legend);
  }

  Calendar.prototype.nextMonth = function() {
    this.current.add('months', 1);
    this.next = true;
    this.draw();
  }

  Calendar.prototype.prevMonth = function() {
    this.current.subtract('months', 1);
    this.next = false;
    this.draw();
  }
  Calendar.prototype.goTo = function(day){
      day = moment(day);
      this.current = moment().set({'year': day.get('year'), 'month': day.get('month'), 'date': 1});
      this.draw();
  }
    
  window.Calendar = Calendar;

  function createElement(tagName, className, innerText) {
    var ele = document.createElement(tagName);
      if(className) {
          ele.className = className;
        }
      
    
    if(innerText) {
      
        if(tagName == 'input'){
          ele.value = innerText;
       } else {
           ele.innderText = ele.textContent = innerText;
       }
    }
    return ele;
  }
  Calendar.prototype.removeBlank  =  function(events){
      var newArr = events.filter(function(ev){
            return  ev.calendar.toLowerCase() !== 'blank';
      });
      return newArr;
  }
    Calendar.prototype.nextMultipleMonth = function (m) {
        this.current.add('months', m);
        this.next = true;
        this.draw();
    }

    Calendar.prototype.prevMultipleMonth = function (m) {
        this.current.subtract('months', m);
        this.next = false;
        this.draw();
    }
}();