import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../api.service';

@Component({ selector:'app-create-circle', standalone:true, imports:[CommonModule, FormsModule, RouterLink], template:`
<section class="page narrow"><div class="page-back"><a routerLink="/dashboard">← Back to dashboard</a></div><div class="eyebrow">ORGANIZER</div><h1>Create a circle</h1><p class="muted">You automatically become the first member and organizer.</p>
<div class="panel"><form (ngSubmit)="submit()" class="form-grid"><label class="span-2">Circle name<input name="name" [(ngModel)]="model.name" required placeholder="e.g. Family Ekub"/></label><label>Contribution (ETB)<input name="amount" type="number" min="1" [(ngModel)]="model.contributionAmount" required/></label><label>Frequency<select name="frequency" [(ngModel)]="model.frequency"><option>Monthly</option><option>Weekly</option></select></label><label>Member limit<input name="limit" type="number" min="2" [(ngModel)]="model.memberLimit" required/></label><label>Start date<input name="date" type="date" [(ngModel)]="model.startDate"/></label><button type="submit" class="btn primary full span-2" [disabled]="loading">{{loading?'Creating…':'Create circle'}}</button></form><div class="error" *ngIf="error">{{error}}</div></div></section>`})
export class CreateCircleComponent { private api=inject(ApiService); private router=inject(Router); model={name:'',contributionAmount:1000,frequency:'Monthly',memberLimit:3,startDate:''}; loading=false;error=''; submit(){
  const name=this.model.name.trim();
  if(!name){this.error='Circle name is required.';return;}
  this.loading=true;this.error='';
  const startDate=this.model.startDate ? `${this.model.startDate}T00:00:00.000Z` : null;
  const body={name,contributionAmount:Number(this.model.contributionAmount),frequency:this.model.frequency,memberLimit:Number(this.model.memberLimit),startDate};
  console.log('Creating circle:', body);
  this.api.createCircle(body).subscribe({next:r=>{console.log('Circle created:', r);this.loading=false;this.router.navigate(['/circles',r.id]);},error:e=>{console.error('Create circle failed:',e);this.error=e?.error?.message||e?.error?.title||'Could not create circle.';this.loading=false;}});
}}
