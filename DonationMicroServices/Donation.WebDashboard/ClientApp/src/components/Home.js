import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip } from 'recharts';

function generate2DigitNumber(n) {
    if (n < 10)
        return `0${n}`;
    return `${n}`;
}
function generateTimeStamp(hour, minute) {
    return `${generate2DigitNumber(hour)}:${generate2DigitNumber(minute)}`;
}
function getRandomInt(max, min) {

    const v = Math.floor(Math.random() * Math.floor(max));
    if (v < min)
        return min;
    return v;
}

function getDonationProcessedPerSecChartData() {
    const data = [];
    for (let i = 0; i < 60; i+=2) {
        data.push({ timeStamp: generateTimeStamp(14, i), donationProcessedPerSec: getRandomInt(120, 80) });
    }
    return data;
}

export class Home extends Component {

    static displayName = Home.name;

    render() {

        const renderLineChart = (
            <LineChart width={400} height={200} data={getDonationProcessedPerSecChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="donationProcessedPerSec" stroke="#8884d8" />
                <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
                <XAxis dataKey="timeStamp" />
                <YAxis />
                <Tooltip />
            </LineChart>
        );

        return (
            <div>
                <h1>Donation Live DashBoard</h1>
                <hr />
                {renderLineChart}
            </div>
        );
    }
}
