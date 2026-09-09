import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

import _ from 'underscore';

@Pipe({
  name: 'simpleSearch'
})


export class SimpleSearchPipe implements PipeTransform {
    transform(value?: any, input?: any, prop?:any) {

        if (input != null && input.length != 0) {
           if(input != ' '){
             input = input.toString().toLowerCase();
            if(prop && prop != '' && value != undefined) {
              let props = prop.split(',');
              return value.filter(function (el: any) {
                for (let p of props) {
                  if (el[p] && el[p].toString().toLowerCase().indexOf(input) > -1) {
                    return true;
                  }
                }
                return false;
              })

            }



           }

        }
        return value;
  }
}

@Pipe({
  name: 'flag'
})

export class flagPipe implements PipeTransform {
    transform(value: any, input: any, prop:any) {


        if (input != null && input.length != 0) {

            //for objects
            if(prop && prop != '' && value != undefined) {
              return value.filter(function (el: any) {
                el = el[prop];
                return el == input;
              })
            }


        }
        return value;
  }
}

@Pipe({
  name: 'visafilter'
})

export class visaFilterPipe implements PipeTransform {
  transform(value: any, input: any) {
    if (input != null && input.length != 0) {

      if(input.visa != undefined){

        if(input.visa.length !=0){

          return value.filter(function (el: any) {

            if(el.visa != null){
              var arr = el.visa.split(",").map((item:any) => item.trim());
              for(let i=0;i<input.visa.length;i++){
                if(arr.indexOf(input.visa[i]) != -1) {
                  return el;
                }
              }
            }

          })

        }
        else {
          return value;
        }

      }

    }
    return value;
  }
}

@Pipe({
  name: 'totalexpfilter'
})

export class totalExpFilterPipe implements PipeTransform {
  transform(value: any, input: any) {

    if (input != null && input.length != 0) {

      if(input.experience != undefined){

        return value.filter(function (el: any) {
          el = el['totalExp'];
          if(input.experience == '4')
            return el >= 0 && el <=4;
          else if(input.experience == '10')
            return el >= 4 && el <=10;
          else if(input.experience == '11')
            return el > 10;
          else
            return value;
        })

      }

    }
    return value;
  }
}

@Pipe({
  name: 'subStr'
})

export class subStringPipe implements PipeTransform {
  transform(value: any, input: number) {
    return value.substring(0,input).toUpperCase();
  }
}

@Pipe({ name: 'keepHtml', pure: false })

export class EscapeHtmlPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {
  }

  transform(content:any) {
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }
}

@Pipe({
  name: 'jobLocation'
})

export class jobLocationPipe implements PipeTransform {
  transform(value: any) {
    if(!_.isEmpty(value)) {
      let location = value[0]
      let name = location.city
      return name.city1 + ', ' + name.stateCode
    }
    else {
      return ""
    }
  }
}