import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip } from 'recharts';
import ReactTable from "react-table";
import "react-table/react-table.css";
import {
    // generate2DigitNumber,
    // generateTimeStamp,
    // getRandomInt,
    getDonationProcessedPerSecChartData,
    getDonationReceivedPerSecChartData
} from './generateDashboardData'

/*
recharts
     http://recharts.org/en-US/examples/BiaxialLineChart
React-Table    
     https://www.npmjs.com/package/react-table
     https://codesandbox.io/s/react-table-simple-table-hpduw
     https://codesandbox.io/s/github/tannerlinsley/react-table/tree/master/archives/v6-examples/react-table-cell-renderers
 */

export class Home extends Component {

    static displayName = Home.name;

    timerId = null;
    refreshTimeOut = 1000 * 4;
    

    state = {
        systemActivitySummary: {
            donationSentToEndPointActivitySummaryTotals: [],
            donationSentToEndPointActivitySummaryDictionary: {},

            donationEnqueuedActivitySummaryDictionary: {},
            dashboardResourceActivitySummaryDictionary: {},
            donationProcessedActivitySummaryDictionary: {},
            donationInfoSummaryDictionary: {},
            donationErrorsSummaryDictionary: {},
            lastMessage: "No message yet",
            autoRefreshOn : false,
        }
    };

    // Return a promise
    reloadData = () => {

        console.log(`About to refresh...`);

        return fetch('api/SystemActivities/GetSystemActivitySummary').then(response => response.json())
            .then(data => {
                console.log(`data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    componentDidMount() {
        
        this.reloadData().then(() => {

            this.setAutoRefresh(true);
        });        
    }

    updateState = (property, value) => {

        this.setState({ ...this.state, [property]: value }, () => {
            //console.log(`state: ${JSON.stringify(this.state)}`);
        });
    }

    reverseAutoRefresh = () => {

        this.setAutoRefresh(!this.state.autoRefreshOn);
    }

    getAutoRefreshStatus = () => {

        return this.state.autoRefreshOn ? "On" : "Off";
    }

    setAutoRefresh = (on) => {

        if (on) {
            this.timerId = setInterval(this.reloadData, this.refreshTimeOut);
        }
        else {
            clearTimeout(this.timerId);
        }
        this.updateState('autoRefreshOn', on);
    }    

    getActivitySummaryTable = (activitySummaryDictionary) => {

        let sTotal         = 0;
        let sItemPerSecond = 0;
        let sCaption       = null;
        const machineNames = Object.keys(activitySummaryDictionary);

        const r = machineNames.map((machineName) => {

            const machineInfo  = activitySummaryDictionary[machineName];
            sTotal            += machineInfo.total;
            sItemPerSecond    += machineInfo.itemPerSecond;

            if (sCaption === null) sCaption = machineInfo.caption;

            return {
                machineName  : machineInfo.machineName,
                caption      : machineInfo.caption,
                total        : machineInfo.total,
                itemPerSecond: machineInfo.itemPerSecond,
                jsonData     : machineInfo.jsonData,
                message      : machineInfo.message, // This is an array of string
            };
        });

        r.push({ // Add summation row
            machineName  : 'Total',
            caption      : sCaption,
            total        : sTotal,
            itemPerSecond: sItemPerSecond,
        });
        return r;
    }

    getDonationInfoActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationInfoSummaryDictionary);
    }

    getDonationErrorsActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationErrorsSummaryDictionary);
    }

    getDonationProcessedpointActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary);
    }

    getDonationSentToEndpointActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary);
    }

    getDonationEnqueuedActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary);
    }

    getDashboardResourceActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.dashboardResourceActivitySummaryDictionary);
    }

    renderDonationInfoActivitySummaryTable = () => {

        const data = this.getDonationInfoActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Donation Info",
                    columns: this.getColumnsForMessagesTable()                        
                }]}
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
                SubComponent={row => {
                    var messages = row.original.message;
                    var messagesHtml = messages.map((message, index) => {
                        return <li key={index}>{message}</li>;
                    });                    
                    return (
                        <ul style={{ padding: "20px" }}>
                            {messagesHtml}
                        </ul>
                    );
                }}
            />
        );
    }

    renderDonationErrorsActivitySummaryTable = () => {

        const data = this.getDonationErrorsActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Errors",
                        columns: this.getColumnsForMessagesTable()
                }]}                
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    getColumnsForMessagesTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName,
        },
        { Header: "Total", accessor: "total" },
        { Header: "Messages", accessor: "message" }
    ]

    getColumnsForDonationPerSecondTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName,
        },
        { Header: "Total", accessor: "total" },
        { Header: "Donation/S", accessor: "itemPerSecond" }
    ];

    renderDonationProcessedActivitySummaryTable = () => {

        const data = this.getDonationProcessedpointActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Processed",
                        columns: this.getColumnsForDonationPerSecondTable()
                }]}                
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    renderDonationSentToEndpointActivitySummaryTable = () => {
        const data = this.getDonationSentToEndpointActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Sent To Endpoint",
                        columns: this.getColumnsForDonationPerSecondTable()
                }]}                
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    // https://www.npmjs.com/package/react-table
    // https://codesandbox.io/s/react-table-simple-table-hpduw
    // With colors
    // https://codesandbox.io/s/github/tannerlinsley/react-table/tree/master/archives/v6-examples/react-table-cell-renderers
    renderDonationEnqueuedActivitySummaryTable = () => {
        const data = this.getDonationEnqueuedActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Donation Enqueued",
                    columns: this.getColumnsForDonationPerSecondTable()                            
                }]}
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    getColumnsForJsonDataTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName.replace("person", ""),
        },
        { Header: "Total", accessor: "total" },
        { Header: "Json Data", accessor: "jsonData" }
    ];
    
    renderDashboardResourceActivitySummaryTable = () => {

        const data = this.getDashboardResourceActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Dashboard Country Aggregate",
                    columns: this.getColumnsForJsonDataTable()
                }]}
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    getDonationSentToEndPointChartData = () => {

        const data = [];
        const totals = this.state.systemActivitySummary.donationSentToEndPointActivitySummaryTotals;
        totals.forEach((info, index) => {
            data.push({ timeStamp: info.label, value: info.value });
        });
        return data;
    };

    render() {
        const donationSentToEndPointChart = (
            <LineChart width={500} height={200} data={this.getDonationSentToEndPointChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="value" stroke="#8884d8" />
                <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
                <XAxis dataKey="timeStamp" />
                <YAxis />
                <Tooltip />
            </LineChart>
        );

        const donationReceivedPerSecChart = (
            <LineChart width={500} height={200} data={getDonationReceivedPerSecChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="donatioReceivedPerSec" stroke="#8884d8" />
                <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
                <XAxis dataKey="timeStamp" />
                <YAxis />
                <Tooltip />
            </LineChart>
        );

        return (
            <div>
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.reverseAutoRefresh} > AutoRefresh: {this.getAutoRefreshStatus()} </button>
                &nbsp;
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.reloadData} > Refresh </button>
                
                <div className="card">
                    <div className="card-header">Donation Sent</div>
                    <div className="card-body">
                        {donationSentToEndPointChart}
                    </div>
                </div>

                {/*
                <div className="card">
                    <div className="card-header">Donation Processed Per Second</div>
                    <div className="card-body">
                        {donationProcessedPerSecChart}
                    </div>
                </div>
                */}

                <div className="card">
                    <div className="card-header">Message</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.state.systemActivitySummary.lastMessage}
                        </h4>
                    </div>
                </div>
                <br /><br />

                {this.renderDonationSentToEndpointActivitySummaryTable()}
                <br /><br />
                {this.renderDonationEnqueuedActivitySummaryTable()}
                <br /><br />
                {this.renderDonationProcessedActivitySummaryTable()}
                <br /><br />
                {this.renderDonationInfoActivitySummaryTable()}
                <br /><br />
                {this.renderDonationErrorsActivitySummaryTable()}
                <br /><br />
                {this.renderDashboardResourceActivitySummaryTable()}
                <br /><br />
                  
            </div>
        );
    }
}

/*

                          <p className="card-text">With supporting text below as a natural lead-in to additional content.</p>
 */
