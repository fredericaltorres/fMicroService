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
            donationSentToEndPointActivitySummaryDictionary: {},
            donationEnqueuedActivitySummaryDictionary: {},
            dashboardResourceActivitySummaryDictionary: {},
            donationProcessedActivitySummaryDictionary: {},
            donationInfoSummaryDictionary: {},
            donationErrorsSummaryDictionary: {},
            lastMessage: "No message yet",
            autoRefreshOn : true,
        }
    };

    reloadData = () => {

        console.log(`About to refresh...`);

        fetch('api/SystemActivities/GetSystemActivitySummary').then(response => response.json())
            .then(data => {
                console.log(`data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    componentDidMount() {

        this.setAutoRefresh(true);
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
        let sTotal = 0;
        let sItemPerSecond = 0;
        let sCaption = null;
        const machineNames = Object.keys(activitySummaryDictionary);

        const r = machineNames.map((machineName) => {

            const machineInfo = activitySummaryDictionary[machineName];
            sTotal += machineInfo.total;
            sItemPerSecond += machineInfo.itemPerSecond;

            if (sCaption === null) sCaption = machineInfo.caption;

            return {
                machineName: machineInfo.machineName,
                caption: machineInfo.caption,
                total: machineInfo.total,
                itemPerSecond: machineInfo.itemPerSecond,
                jsonData: machineInfo.jsonData,
                message: machineInfo.message, // This is an array of string
            };
        });

        if (machineNames.length > 0) {
            sItemPerSecond = sItemPerSecond / machineNames.length; // compute average
        }

        r.push({ // Add summation row
            machineName: 'Total',
            caption: sCaption,
            total: sTotal,
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
                columns={[
                    {
                        Header: "Donation Info",
                        columns: [
                            {
                                Header: "Machine Name",
                                id: "machineName",
                                accessor: d => d.machineName,
                            },
                            //{ Header: "Activity", accessor: "caption" },
                            { Header: "Total", accessor: "total" },
                            { Header: "Messages", accessor: "message" } // ++++
                        ]
                    }
                ]}
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
                columns={[
                    {
                        Header: "Donation Errors",
                        columns: [
                            {
                                Header: "Machine Name",
                                id: "machineName",
                                accessor: d => d.machineName,
                            },
                            //{ Header: "Activity", accessor: "caption" },
                            { Header: "Total", accessor: "total" },
                            { Header: "Messages", accessor: "message" }
                        ]
                    }
                ]}
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    renderDonationProcessedActivitySummaryTable = () => {

        const data = this.getDonationProcessedpointActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[
                    {
                        Header: "Donation Processed",
                        columns: [
                            {
                                Header: "Machine Name",
                                id: "machineName",
                                accessor: d => d.machineName,
                            },
                            //{ Header: "Activity", accessor: "caption" },
                            { Header: "Total", accessor: "total" },
                            { Header: "Donation/S", accessor: "itemPerSecond" }
                        ]
                    }
                ]}
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
                columns={[
                    {
                        Header: "Donation Sent To Endpoint",
                        columns: [
                            {
                                Header: "Machine Name",
                                id: "machineName",
                                accessor: d => d.machineName.replace("person", ""),
                                // minWidth: 150
                            },
                            //{ Header: "Activity", accessor: "caption" },
                            { Header: "Total", accessor: "total" },
                            { Header: "Donation/S", accessor: "itemPerSecond" }
                        ]
                    }
                ]}
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
                columns={[
                    {
                        Header: "Donation Enqueued",
                        columns: [
                            {
                                Header: "Machine Name",
                                id: "machineName",
                                accessor: d => d.machineName.replace("donation-restapi-entrance-", ""),
                                // minWidth: 230
                            },
                            //{ Header: "Activity", accessor: "caption" },
                            { Header: "Total", accessor: "total" },
                            { Header: "Donation/S", accessor: "itemPerSecond" }
                        ]
                    }
                ]}
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
    renderDashboardResourceActivitySummaryTable = () => {
        const data = this.getDashboardResourceActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[
                    {
                        Header: "Dashboard Country Aggregate",
                        columns: [
                            {
                                Header: "Machine Name",
                                id: "machineName",
                                accessor: d => d.machineName.replace("person", ""),
                                // minWidth: 150
                            },
                            //{ Header: "Activity", accessor: "caption" },
                            { Header: "Total", accessor: "total" },
                            { Header: "Json Data", accessor: "jsonData" }
                        ]
                    }
                ]}
                defaultPageSize={3}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }


    render() {

        const donationProcessedPerSecChart = (
            <LineChart width={500} height={200} data={getDonationProcessedPerSecChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="donationProcessedPerSec" stroke="#8884d8" />
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
                {/*
                <div className="card">
                    <div className="card-header">Donation Received Per Second</div>
                    <div className="card-body">
                        {donationReceivedPerSecChart}
                    </div>
                </div>

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

                <div className="card">
                    <div className="card-header">Donation Sent To Endpoint</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.renderDonationSentToEndpointActivitySummaryTable()}
                        </h4>
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">Donation Enqueued</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.renderDonationEnqueuedActivitySummaryTable()}
                        </h4>
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">Donation Processed</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.renderDonationProcessedActivitySummaryTable()}
                        </h4>
                    </div>
                </div>
                
                <div className="card">
                    <div className="card-header">Donation Info</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.renderDonationInfoActivitySummaryTable()}
                        </h4>
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">Donation Errors</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.renderDonationErrorsActivitySummaryTable()}
                        </h4>
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">Donation Data Dashboard</div>
                    <div className="card-body">
                        <h4 className="card-title">
                            {this.renderDashboardResourceActivitySummaryTable()}
                        </h4>
                    </div>
                </div>
            </div>
        );
    }
}

/*

                          <p className="card-text">With supporting text below as a natural lead-in to additional content.</p>
 */
