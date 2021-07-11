import React from 'react';
import {Header, Menu} from "semantic-ui-react";
import Calendar from "react-calendar";

export default function ActivityFilter(){
    return(
        <>
            <Menu vertical size='large' style={{width: '100%', marginTop: 26, borderRadius: 4}}>
                <Header icon='filter' attached color='teal' content='Filters' style={{borderRadius: 4}}/>
                <Menu.Item content='All Activities'/>
                <Menu.Item content="I'm going"/>
                <Menu.Item content="I'm hosting"/>
            </Menu>
            <Header/>
            <Calendar/>
        </>

    )
}